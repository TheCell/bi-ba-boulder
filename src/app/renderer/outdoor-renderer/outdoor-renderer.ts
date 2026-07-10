import {
  AfterViewInit,
  Component,
  DestroyRef,
  effect,
  ElementRef,
  HostListener,
  inject,
  input,
  signal,
  ViewChild
} from '@angular/core';
import { KeyboardShortcutsModule } from 'ng-keyboard-shortcuts';
import * as THREE from 'three';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { CameraControlsService } from '../camera-controls.service';
import { fitCameraToCenteredObject } from '../common/camera-utils';
import { ColorService } from '../../core/util-services/color.service';
import { LineDto } from '@api-net/model/models';

@Component({
  selector: 'app-outdoor-renderer',
  imports: [KeyboardShortcutsModule],
  templateUrl: './outdoor-renderer.html',
  styleUrl: './outdoor-renderer.scss'
})
export class OutdoorRenderer implements AfterViewInit {
  private el: ElementRef = inject(ElementRef);
  private destroyRef = inject(DestroyRef);
  private cameraControlsService = inject(CameraControlsService);
  private colorService = inject(ColorService);

  @ViewChild('canvas') public canvas: ElementRef = null!;
  @HostListener('window:resize') public onResize(): void {
    if (this.renderer) {
      const canvasSizes = {
        width: this.el.nativeElement.offsetWidth,
        height: this.el.nativeElement.offsetHeight
      };

      this.renderer.setPixelRatio(window.devicePixelRatio);
      this.renderer.setSize(canvasSizes.width, canvasSizes.height);
      this.camera.aspect = canvasSizes.width / canvasSizes.height;
      this.camera.updateProjectionMatrix();
      this.loop();
    }
  }

  public rawModel = input<ArrayBuffer>();
  public lines = input<LineDto[]>();

  private proccessedRawModel = signal<ArrayBuffer | undefined>(undefined);
  private scene = new THREE.Scene();
  private loader = new GLTFLoader();
  private camera: THREE.PerspectiveCamera = null!;
  private controls: OrbitControls = null!;
  private renderer: THREE.WebGLRenderer = null!;
  private ambientLightIntensity = 2.0;
  private ambientLightLowIntensity = 2.0;
  private directionalLightIntensity = 1.0;
  private ambientLight: THREE.AmbientLight = new THREE.AmbientLight(0xffffff, this.ambientLightIntensity);
  private directionalLight = new THREE.DirectionalLight(0xffffff, this.directionalLightIntensity); // this is for shadows

  // all the debugging stuff
  private lineGeometry = new THREE.BufferGeometry();
  private line = new THREE.Line(this.lineGeometry, new THREE.LineBasicMaterial());
  private currentMesh?: THREE.Mesh;

  // tube
  private tubeParams = {
    radius: 0.05,
    extrusionSegments: 100,
    radiusSegments: 6
  };

  private tubeGeometries: THREE.TubeGeometry[] = [];
  private tubeMeshes: THREE.Mesh[] = [];
  private rayVisionTubeMeshes: THREE.Mesh[] = [];

  // Shader material related
  private originalBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockTexture: THREE.Texture | null = null;

  private currentGltf?: GLTF;
  private initialized = false; // temporary 'fix' for a timing problem

  public constructor() {
    effect(() => {
      const rawModel = this.rawModel();
      if (rawModel !== this.proccessedRawModel()) {
        this.proccessedRawModel.set(rawModel);
        if (rawModel !== undefined) {
          // this effect can run through before afterInit is finished. Needs fixing.
          this.removePreviousAndAddBoulderToScene(rawModel);
        }
      }
    });

    effect(() => {
      this.lines();
      this.regenerateLines();
    });

    // effect(() => {
    //   const boulderProblem = this.boulderProblem();

    //   if (boulderProblem) {
    //     this.setHighlightedHoldsTextureFromData(boulderProblem.image, 128, 128);
    //     this.highlightActiveShaderUniform = 1.0;
    //     this.ambientLight.intensity = this.ambientLightLowIntensity;
    //   } else {
    //     this.highlightedHoldsTexture = undefined;
    //     this.highlightActiveShaderUniform = 0.0;
    //     this.ambientLight.intensity = this.ambientLightIntensity;
    //   }
    //   this.loop();
    // });

    // const activatedRoute = inject(ActivatedRoute);
    // this.rgbBlockTexture = activatedRoute.snapshot.data['spraywallDebugTexture'];

    this.destroyRef.onDestroy(() => this.dispose());
  }

  public ngAfterViewInit(): void {
    this.createCanvas();

    // const lines = this.lines();
    // if (lines !== this.processedLines) {
    //   if (lines !== undefined) {
    //     lines.forEach((line: BoulderLine) => {
    //       this.addLineToScene(
    //         this.scene,
    //         line.points.map((point) => new THREE.Vector3(point.x, point.y, point.z)),
    //         line.color
    //       );
    //     });

    //     this.processedLines = lines;
    //   }
    // }
    this.initialized = true;
    this.resetCameraPosition();
  }

  private createCanvas(): void {
    const canvas = this.canvas.nativeElement;
    if (!canvas) {
      return;
    }

    const canvasSizes = {
      width: canvas.offsetWidth,
      height: canvas.offsetHeight
    };

    this.renderer = new THREE.WebGLRenderer({
      logarithmicDepthBuffer: true,
      canvas: canvas,
      alpha: true
    });
    this.renderer.setClearColor(0x000000, 0);

    this.camera = new THREE.PerspectiveCamera(75, canvasSizes.width / canvasSizes.height, 0.001, 1000);
    this.camera.layers.enable(0);
    this.camera.layers.enable(1);

    this.onResize();
    this.scene.add(this.camera);
    this.scene.add(this.directionalLight);
    this.scene.add(this.ambientLight);
    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    this.controls.mouseButtons = {
      LEFT: THREE.MOUSE.PAN,
      MIDDLE: THREE.MOUSE.DOLLY,
      RIGHT: THREE.MOUSE.ROTATE
    };
    this.controls.touches = {
      ONE: THREE.TOUCH.PAN,
      TWO: THREE.TOUCH.DOLLY_ROTATE
    };
    this.controls.addEventListener('change', this.loop);

    this.lineGeometry.setFromPoints([new THREE.Vector3(), new THREE.Vector3()]);
    this.scene.add(this.line);
    this.regenerateLines();

    this.loop();
  }

  private loop = () => {
    if (!this.renderer) {
      return;
    }

    // if (this.rgbBlockMaterial) {
    //   const shader = this.rgbBlockMaterial.userData['shader'];

    //   if (shader) {
    //     shader.uniforms.useRgbTexture.value = this.useRgbTexture;
    //     shader.uniforms.highlightedHoldsTexture.value = this.highlightedHoldsTexture;
    //     shader.uniforms.isHighlightActive.value = this.highlightActiveShaderUniform;
    //   }
    // }

    this.renderer.render(this.scene, this.camera);
    // window.requestAnimationFrame(this.loop); // removed to not rerender on idle
  };

  private removePreviousAndAddBoulderToScene(buffer: ArrayBuffer): void {
    this.loader.parse(
      buffer,
      '',
      (gltf: GLTF) => {
        this.scene.add(gltf.scene);
        // let childCounter = 0;
        gltf.scene.traverse((child) => {
          child.layers.set(1);
          // childCounter++;
          const mesh = child as THREE.Mesh;
          if (mesh.isMesh) {
            this.currentMesh = mesh;
            this.originalBlockMaterial = mesh.material as THREE.MeshPhysicalMaterial;
            this.originalBlockTexture = this.originalBlockMaterial.map;
            this.originalBlockMaterial.needsUpdate = true;
            // this.originalBlockTexture!.needsUpdate = true;
            // this.originalBlockTexture!.colorSpace = THREE.LinearSRGBColorSpace;
            // this.renderer.outputColorSpace = THREE.LinearSRGBColorSpace;
            // // this.originalBlockMaterial.wireframe = true;
            // this.rgbBlockMaterial = this.setupCustomShaderMaterial();
          }
        });

        if (this.currentGltf !== undefined) {
          this.removeBoulderFromScene(this.currentGltf);
          this.currentGltf = gltf;
        } else {
          this.currentGltf = gltf;
          this.resetCameraPosition();
        }
        // this.setupHighlightTexture(); // we don't know when the model is loaded, so try to swap here (no-op if model not loaded yet)
        this.loop();
      },
      (err: ErrorEvent) => {
        throw new Error(err.message);
      }
    );
  }

  private regenerateLines(): void {
    if (!this.initialized) {
      return;
    }

    for (const tubeMesh of this.tubeMeshes) {
      this.scene.remove(tubeMesh);
    }
    this.tubeMeshes = [];

    for (const tubeGeometry of this.tubeGeometries) {
      tubeGeometry.dispose();
    }
    this.tubeGeometries = [];

    for (const rayVisionTubeMesh of this.rayVisionTubeMeshes) {
      this.scene.remove(rayVisionTubeMesh);
    }
    this.rayVisionTubeMeshes = [];

    const lines = this.lines();
    if (lines === undefined) {
      return;
    }

    for (const line of lines) {
      if (line.data?.positions === undefined || line.data.positions.length < 3) {
        continue;
      }

      const tubeMaterial = new THREE.MeshBasicMaterial({
        color: this.colorService.nextColor(),
        transparent: true,
        opacity: 0.3,
        depthTest: false,
        depthWrite: false
      });
      const rayVisionMaterial = new THREE.MeshStandardMaterial({
        color: tubeMaterial.color
      });

      const path = new THREE.CatmullRomCurve3(
        line.data?.positions.map((point) => new THREE.Vector3(point[0], point[1], point[2])),
        false,
        'chordal',
        0.5
      );

      const tubeGeometry = new THREE.TubeGeometry(
        path,
        this.tubeParams.extrusionSegments,
        this.tubeParams.radius,
        this.tubeParams.radiusSegments,
        false
      );
      const tubeMesh: THREE.Mesh = new THREE.Mesh(tubeGeometry, tubeMaterial);
      const rayVisionTubeMesh = tubeMesh.clone();
      rayVisionTubeMesh.material = rayVisionMaterial;
      this.tubeGeometries.push(tubeGeometry);
      this.tubeMeshes.push(tubeMesh);
      this.rayVisionTubeMeshes.push(rayVisionTubeMesh);
      this.scene.add(rayVisionTubeMesh);
      this.scene.add(tubeMesh);
    }

    this.loop();
  }

  private resetCameraPosition(): void {
    if (this.initialized && this.currentGltf) {
      fitCameraToCenteredObject(this.camera, this.currentGltf.scene, 0, this.controls);
      this.cameraControlsService.setOrbitControls(this.controls);
    }
  }

  private removeBoulderFromScene(gltf: GLTF): void {
    gltf.scene.traverse((child) => {
      const mesh = child as THREE.Mesh;
      if (mesh.isMesh) {
        mesh.geometry?.dispose();
      }
    });
    this.scene.remove(gltf.scene);
  }

  private dispose(): void {
    this.controls?.removeEventListener('change', this.loop);
    this.controls?.dispose();

    if (this.currentGltf) {
      this.removeBoulderFromScene(this.currentGltf);
    }

    this.scene.traverse((child) => {
      const mesh = child as THREE.Mesh;
      if (mesh.isMesh) {
        mesh.geometry?.dispose();
      }
    });

    // this.highlightedHoldsTexture?.dispose();
    // this.rgbBlockTexture?.dispose();
    // this.rgbBlockMaterial?.dispose();
    this.renderer?.dispose();
  }
}

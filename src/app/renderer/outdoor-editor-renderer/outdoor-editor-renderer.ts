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
import { KeyboardShortcutsModule, ShortcutEventOutput, ShortcutInput } from 'ng-keyboard-shortcuts';
import * as THREE from 'three';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { CameraControlsService } from '../camera-controls.service';
import { fitCameraToCenteredObject } from '../common/camera-utils';
import { ColorService } from '../../core/util-services/color.service';

@Component({
  selector: 'app-outdoor-editor-renderer',
  imports: [KeyboardShortcutsModule],
  templateUrl: './outdoor-editor-renderer.html',
  styleUrl: './outdoor-editor-renderer.scss'
})
export class OutdoorEditorRenderer implements AfterViewInit {
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
  public shortcuts: ShortcutInput[] = [];

  // all the debugging stuff
  private raycaster: THREE.Raycaster = null!;
  private mouseHelper = new THREE.Mesh(new THREE.BoxGeometry(1, 1, 10), new THREE.MeshNormalMaterial());
  private lineGeometry = new THREE.BufferGeometry();
  private line = new THREE.Line(this.lineGeometry, new THREE.LineBasicMaterial());
  private currentMesh?: THREE.Mesh;
  private intersection = {
    intersects: false,
    point: new THREE.Vector3(),
    normal: new THREE.Vector3()
  };
  private currentIntersections: THREE.Intersection<THREE.Object3D<THREE.Object3DEventMap>>[] = [];

  private position = new THREE.Vector3();
  private orientation = new THREE.Euler();
  private loggedPoints: THREE.Vector3[] = [];
  private sphereArray: THREE.Mesh[] = [];

  // tube
  private tubeParams = {
    radius: 0.05,
    extrusionSegments: 100,
    radiusSegments: 6
  };
  // points
  private pointParams = {
    radius: 0.1
  };
  private tubeGeometry?: THREE.TubeGeometry;
  private tubeMaterial = new THREE.MeshBasicMaterial({
    color: this.colorService.nextColor(),
    transparent: true,
    opacity: 0.3,
    depthTest: false,
    depthWrite: false
  });
  private rayVisionMaterial = new THREE.MeshStandardMaterial({
    color: this.tubeMaterial.color
  });
  private tubeMesh?: THREE.Mesh;
  private rayVisionTubeMesh?: THREE.Mesh;
  // debugging stuff end

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

    // raycaster things
    this.mouseHelper.visible = false;
    window.addEventListener('pointermove', this.onPointerMove);
    window.addEventListener('pointerdown', this.addPointToLoggedPoints);

    this.shortcuts.push({
      key: ['ctrl + z'],
      preventDefault: true,
      command: (_: ShortcutEventOutput) => this.removeLastPoint()
    });
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

  public removeLastPoint(): void {
    console.log('asdlkfj');

    this.loggedPoints.pop();
    const sphere = this.sphereArray.pop();
    if (sphere) {
      this.scene.remove(sphere);
    }
    this.regeneratePath();
  }

  // more debugging stuff
  private onPointerMove = (event: PointerEvent) => {
    if (!this.renderer || !this.camera || !this.raycaster) {
      return;
    }

    this.checkIntersection(event.clientX, event.clientY);
    this.loop();
  };

  private addPointToLoggedPoints = (event: PointerEvent) => {
    if (!this.intersection.intersects || event.button === 2) {
      return;
    }

    this.position.copy(this.intersection.point);
    this.orientation.copy(this.mouseHelper.rotation);

    this.loggedPoints.push(this.position.clone());
    this.generatePoint(this.position);
    this.regeneratePath();
  };

  private generatePoint(point: THREE.Vector3): void {
    const sphere = new THREE.Mesh(
      new THREE.SphereGeometry(this.pointParams.radius, 16, 16),
      new THREE.MeshNormalMaterial()
    );
    if (this.loggedPoints.length > 0) {
      sphere.position.copy(point);
      this.sphereArray.push(sphere);
      this.scene.add(sphere);
    }
  }

  private regeneratePath(): void {
    console.log(this.currentIntersections);

    if (this.tubeMesh) {
      this.scene.remove(this.tubeMesh);
      this.tubeMesh = undefined;
    }

    if (this.rayVisionTubeMesh) {
      this.scene.remove(this.rayVisionTubeMesh);
      this.rayVisionTubeMesh = undefined;
    }

    if (this.loggedPoints.length > 2) {
      if (this.tubeGeometry) {
        this.tubeGeometry.dispose();
        this.tubeGeometry = undefined;
      }

      const path = new THREE.CatmullRomCurve3(this.loggedPoints, false, 'chordal', 0.5);
      this.tubeGeometry = new THREE.TubeGeometry(
        path,
        this.tubeParams.extrusionSegments,
        this.tubeParams.radius,
        this.tubeParams.radiusSegments,
        false
      );
      this.tubeMesh = new THREE.Mesh(this.tubeGeometry, this.tubeMaterial);
      this.rayVisionTubeMesh = this.tubeMesh.clone();
      this.rayVisionTubeMesh.material = this.rayVisionMaterial;
      this.scene.add(this.rayVisionTubeMesh);
      this.scene.add(this.tubeMesh);
    }

    this.loop();
  }

  private checkIntersection(x: number, y: number) {
    if (this.currentMesh === undefined) {
      return;
    }

    const pointer = new THREE.Vector2();
    const canvasWidth = this.canvas.nativeElement.offsetWidth;
    const canvasHeight = this.canvas.nativeElement.offsetHeight;
    const canvasTop = this.canvas.nativeElement.getBoundingClientRect().top;
    const canvasLeft = this.canvas.nativeElement.getBoundingClientRect().left;

    const mouseX = x - canvasLeft;
    const mouseY = y - canvasTop;

    pointer.x = (mouseX / canvasWidth) * 2 - 1;
    pointer.y = -(mouseY / canvasHeight) * 2 + 1;

    this.raycaster.setFromCamera(pointer, this.camera);
    this.currentIntersections.length = 0;
    this.raycaster.intersectObject(this.currentMesh, false, this.currentIntersections);

    if (this.currentIntersections.length > 0) {
      const currentIntersection = this.currentIntersections[0];
      const point = currentIntersection.point;
      this.mouseHelper.position.copy(point);
      this.intersection.point.copy(point);

      const normalMatrix = new THREE.Matrix3().getNormalMatrix(this.currentMesh.matrixWorld);

      const normal = currentIntersection.face!.normal.clone();
      normal.applyNormalMatrix(normalMatrix);
      normal.multiplyScalar(10);
      normal.add(point);

      this.intersection.normal.copy(currentIntersection.face!.normal);
      this.mouseHelper.lookAt(normal);

      const positions = this.line.geometry.attributes['position'];
      positions.setXYZ(0, point.x, point.y, point.z);
      positions.setXYZ(1, normal.x, normal.y, normal.z);
      positions.needsUpdate = true;

      this.intersection.intersects = true;
      this.currentIntersections.length = 0;
    }
  }
  // more debugging stuff end

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

    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(1);

    this.scene.add(this.mouseHelper);
    this.lineGeometry.setFromPoints([new THREE.Vector3(), new THREE.Vector3()]);
    this.scene.add(this.line);

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


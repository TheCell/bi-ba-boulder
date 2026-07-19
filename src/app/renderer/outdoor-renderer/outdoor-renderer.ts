import {
  AfterViewInit,
  Component,
  DestroyRef,
  effect,
  ElementRef,
  HostListener,
  inject,
  input,
  output,
  signal,
  ViewChild
} from '@angular/core';
import { KeyboardShortcutsModule } from 'ng-keyboard-shortcuts';
import * as THREE from 'three';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { CameraControlsService } from '../camera-controls.service';
import { fitCameraToCenteredObject } from '../common/camera-utils';
import { LineDto } from '@api-net/model/models';

export interface EnhancedLine extends LineDto {
  lineColor: THREE.Color;
}

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
  public lines = input<EnhancedLine[]>();
  public selectedLine = input<{ line: LineDto; setFocus: boolean } | undefined>();
  public selected = output<{ line: LineDto; setFocus: boolean } | undefined>();

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
  private pointerClickStartTimeStamp = 0;

  private currentMesh?: THREE.Mesh;
  private raycaster: THREE.Raycaster = null!;
  private LINE_LAYER = 2;

  // tube
  private tubeParams = {
    radius: 0.05,
    extrusionSegments: 100,
    radiusSegments: 6
  };
  private highlightedTubeParams = {
    radius: 0.1,
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

    effect(() => {
      const lineWithInfos = this.selectedLine();
      if (lineWithInfos === undefined) {
        return;
      }

      const lineObject = this.tubeMeshes.find((tubeMesh) => tubeMesh.userData['id'] === lineWithInfos.line.id);
      if (lineObject === undefined) {
        return;
      }

      if (lineWithInfos.setFocus) {
        this.cameraControlsService.focusOnObject(lineObject);
      }
      this.regenerateLines();
    });

    this.destroyRef.onDestroy(() => this.dispose());
  }

  public ngAfterViewInit(): void {
    this.createCanvas();
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
    this.camera.layers.enable(this.LINE_LAYER);

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

    canvas.addEventListener('pointerdown', () => (this.pointerClickStartTimeStamp = performance.now()));
    canvas.addEventListener('pointerup', (event: PointerEvent) => {
      if (this.pointerClickStartTimeStamp > 0 && performance.now() - this.pointerClickStartTimeStamp < 100) {
        this.onPointerClick(event);
      }
      this.pointerClickStartTimeStamp = 0;
    });

    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(this.LINE_LAYER);

    this.regenerateLines();

    this.loop();
  }

  private loop = () => {
    if (!this.renderer) {
      return;
    }

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
    if (this.scene === undefined) {
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
    const selectedLine = this.selectedLine();

    if (selectedLine !== undefined) {
      const enhancedLine = this.lines()?.find((line) => line.id === selectedLine.line.id);
      if (enhancedLine !== undefined) {
        this.addLineToScene(enhancedLine, true);
      }
    } else {
      for (const line of lines) {
        this.addLineToScene(line, false);
      }
    }

    this.loop();
  }

  private addLineToScene(line: EnhancedLine, isHighlighted: boolean): void {
    if (line.data?.positions === undefined || line.data.positions.length < 3) {
      return;
    }

    // todo add additional info (start holds highlight etc.)

    const tubeMaterial = new THREE.MeshBasicMaterial({
      color: line.lineColor,
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
      isHighlighted ? this.highlightedTubeParams.extrusionSegments : this.tubeParams.extrusionSegments,
      isHighlighted ? this.highlightedTubeParams.radius : this.tubeParams.radius,
      isHighlighted ? this.highlightedTubeParams.radiusSegments : this.tubeParams.radiusSegments,
      false
    );
    const tubeMesh: THREE.Mesh = new THREE.Mesh(tubeGeometry, tubeMaterial);
    tubeMesh.layers.set(this.LINE_LAYER);
    tubeMesh.userData = { id: line.id, identifier: line.identifier };
    const rayVisionTubeMesh = tubeMesh.clone();
    rayVisionTubeMesh.material = rayVisionMaterial;
    rayVisionTubeMesh.layers.set(this.LINE_LAYER);
    this.tubeGeometries.push(tubeGeometry);
    this.tubeMeshes.push(tubeMesh);
    this.rayVisionTubeMeshes.push(rayVisionTubeMesh);
    this.scene.add(rayVisionTubeMesh);
    this.scene.add(tubeMesh);
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

  private onPointerClick = (event: PointerEvent) => {
    if (!this.renderer || !this.camera) {
      return;
    }

    if (event.button === 0) {
      this.checkIntersection(event.clientX, event.clientY);
    }
  };

  private checkIntersection(x: number, y: number): void {
    if (this.tubeMeshes.length === 0) {
      return;
    }

    const lines = this.lines();
    if (lines === undefined || lines.length === 0) {
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
    const currentIntersections: THREE.Intersection<THREE.Object3D<THREE.Object3DEventMap>>[] = [];
    this.raycaster.intersectObjects(this.tubeMeshes, false, currentIntersections);

    if (currentIntersections.length > 0) {
      const currentIntersection = currentIntersections[0];
      if (currentIntersection.object.userData['id'] !== undefined) {
        const selectedLine = lines.find((line) => line.id === currentIntersection.object.userData['id']);

        if (selectedLine !== undefined) {
          this.selected.emit({ line: selectedLine, setFocus: false });
        }
      }
    } else {
      this.selected.emit(undefined);
    }
  }

  private dispose(): void {
    this.controls?.removeEventListener('change', this.loop);
    this.controls?.dispose();
    this.canvas?.nativeElement?.removeEventListener('pointerdown', this.onPointerClick);

    if (this.currentGltf) {
      this.removeBoulderFromScene(this.currentGltf);
    }

    this.scene.traverse((child) => {
      const mesh = child as THREE.Mesh;
      if (mesh.isMesh) {
        mesh.geometry?.dispose();
      }
    });

    this.renderer?.dispose();
  }
}

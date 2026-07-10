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
// import { TransformControls } from 'three/examples/jsm/controls/TransformControls.js';
import { CameraControlsService } from '../camera-controls.service';
import { fitCameraToCenteredObject } from '../common/camera-utils';
import { ColorService } from '../../core/util-services/color.service';
import { Viewpoint } from '../common/viewpoint';
import { VertexNormalsHelper } from 'three/addons/helpers/VertexNormalsHelper.js';
import { DragControls } from 'three/addons/controls/DragControls.js';

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
      this.startLooping();
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
  private loggedPoints: { id: string; position: THREE.Vector3 }[] = [];
  private sphereArray: THREE.Mesh[] = [];
  // private transformControls?: TransformControls;
  private dragControls?: DragControls;
  private isDragging = false;
  private isLooping = false;

  // debugging configs
  private debugColor = 0x98ff98;
  private displayNormals = false;

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

  private debugSphere = new THREE.Mesh(
    new THREE.SphereGeometry(1, 32, 32),
    new THREE.MeshBasicMaterial({ color: this.debugColor })
  );

  private viewpoints: Record<string, Viewpoint> = {
    seitensprung: {
      position: new THREE.Vector3(-9.44876021135404, 7.320794224154875, 6.724980613386679),
      target: new THREE.Vector3(-3.0950377146341763, 7.692263096560984, -2.217579333053568)
    },
    overview: {
      position: new THREE.Vector3(-0.0967487267161844, 9.600426820701172, 11.401826642615301),
      target: new THREE.Vector3(3.710864794256258, 5.128171870749298, -1.8769139834889017)
    }
  };
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

    this.shortcuts.push(
      {
        key: ['ctrl + z'],
        preventDefault: true,
        command: (_: ShortcutEventOutput) => this.removeLastPoint()
      },
      {
        key: ['1'],
        preventDefault: true,
        command: (_: ShortcutEventOutput) => this.goToView('overview')
      },
      {
        key: ['2'],
        preventDefault: true,
        command: (_: ShortcutEventOutput) => this.goToView('seitensprung')
      }
    );
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

    if (this.isDragging) {
      return;
    }

    this.checkIntersection(event.clientX, event.clientY);
    // todo uncommment
    this.startLooping();
  };

  private addPointToLoggedPoints = (event: PointerEvent) => {
    if (!this.intersection.intersects || event.button === 2) {
      return;
    }

    this.position.copy(this.intersection.point);
    this.orientation.copy(this.mouseHelper.rotation);

    const loggedPoint = { id: crypto.randomUUID(), position: this.position.clone() };
    this.loggedPoints.push(loggedPoint);
    this.generatePoint(loggedPoint.id, loggedPoint.position);
    this.regeneratePath();
  };

  private generatePoint(uuid: string, point: THREE.Vector3): void {
    const sphere = new THREE.Mesh(
      new THREE.SphereGeometry(this.pointParams.radius, 16, 16),
      new THREE.MeshNormalMaterial()
    );
    sphere.uuid = uuid;

    if (this.loggedPoints.length > 0) {
      sphere.position.copy(point);
      this.sphereArray.push(sphere);
      this.scene.add(sphere);
      // this.transformControls?.attach(sphere);
    }
  }

  private regeneratePath(): void {
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

      const path = new THREE.CatmullRomCurve3(
        this.loggedPoints.map((lp) => lp.position),
        false,
        'chordal',
        0.5
      );
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

    this.startLooping();
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
    // todo find a solution for this
    this.controls.addEventListener('change', this.startLooping);

    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(1);

    this.scene.add(this.mouseHelper);
    this.lineGeometry.setFromPoints([new THREE.Vector3(), new THREE.Vector3()]);
    this.scene.add(this.line);
    this.scene.add(this.debugSphere);

    // this.transformControls = new TransformControls(this.camera, this.renderer.domElement);
    this.dragControls = new DragControls(this.sphereArray, this.camera, this.renderer.domElement);
    this.dragControls.addEventListener('dragstart', () => {
      this.isDragging = true;
      this.intersection.intersects = false;
      this.cameraControlsService.setCameraInteractable(false);
    });

    this.dragControls.addEventListener('drag', (event) => {
      this.loggedPoints.find((lp) => lp.id === event.object.uuid)?.position.copy(event.object.position);
      this.regeneratePath();
      this.startLooping();
    });

    this.dragControls.addEventListener('dragend', () => {
      this.isDragging = false;
      this.cameraControlsService.setCameraInteractable(true);
    });

    // this.scene.add(this.transformControls);

    this.startLooping();
  }

  private loop = () => {
    if (!this.renderer || !this.isLooping) {
      return;
    }

    this.isLooping = false;

    // if (this.rgbBlockMaterial) {
    //   const shader = this.rgbBlockMaterial.userData['shader'];

    //   if (shader) {
    //     shader.uniforms.useRgbTexture.value = this.useRgbTexture;
    //     shader.uniforms.highlightedHoldsTexture.value = this.highlightedHoldsTexture;
    //     shader.uniforms.isHighlightActive.value = this.highlightActiveShaderUniform;
    //   }
    // }

    if (this.controls) {
      // console.log('this.controls.target', this.controls.target, 'this.camera.position', this.camera.position);
      this.debugSphere.position.copy(this.controls.target);
    }

    if (this.cameraControlsService.needsAnimation) {
      this.cameraControlsService.animateTransition();
      this.isLooping = true; // keep looping until the animation is finished
    }

    this.renderer.render(this.scene, this.camera);
    if (this.isLooping) {
      window.requestAnimationFrame(this.loop);
      // this.loop();
    }
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
            if (this.displayNormals) {
              const normalsMesh = new VertexNormalsHelper(mesh, 0.5, this.debugColor);
              this.scene.add(normalsMesh);
            }
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
        this.startLooping();
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
    this.controls?.removeEventListener('change', this.startLooping);
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

  public goToView(viewName: string): void {
    const viewpoint = this.viewpoints[viewName];

    if (viewpoint) {
      this.cameraControlsService.goToView(viewpoint);
      this.startLooping();
    }
  }

  private startLooping = () => {
    if (this.isLooping) {
      return;
    }

    this.isLooping = true;
    this.loop();
  };
}

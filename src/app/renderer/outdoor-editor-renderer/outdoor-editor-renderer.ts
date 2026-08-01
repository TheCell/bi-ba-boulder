import {
  AfterViewInit,
  Component,
  DestroyRef,
  effect,
  ElementRef,
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
import { Viewpoint } from '../common/viewpoint';
import { VertexNormalsHelper } from 'three/addons/helpers/VertexNormalsHelper.js';
import { DragControls } from 'three/addons/controls/DragControls.js';
import { LineDto } from '@api-net/model/models';
import {
  computeBoundsTree,
  disposeBoundsTree,
  acceleratedRaycast,
  disposeBatchedBoundsTree,
  computeBatchedBoundsTree,
  GeometryBVH
} from 'three-mesh-bvh';

THREE.BufferGeometry.prototype.computeBoundsTree = computeBoundsTree;
THREE.BufferGeometry.prototype.disposeBoundsTree = disposeBoundsTree;
THREE.Mesh.prototype.raycast = acceleratedRaycast;

THREE.BatchedMesh.prototype.computeBoundsTree = computeBatchedBoundsTree;
THREE.BatchedMesh.prototype.disposeBoundsTree = disposeBatchedBoundsTree;
THREE.BatchedMesh.prototype.raycast = acceleratedRaycast;


interface LoggedPoint {
  id: string;
  position: THREE.Vector3;
}
@Component({
  selector: 'app-outdoor-editor-renderer',
  imports: [KeyboardShortcutsModule],
  templateUrl: './outdoor-editor-renderer.html',
  styleUrl: './outdoor-editor-renderer.scss',
  host: {
    '(window:resize)': 'onResize()'
  }
})
export class OutdoorEditorRenderer implements AfterViewInit {
  private readonly el: ElementRef = inject(ElementRef);
  private readonly destroyRef: DestroyRef = inject(DestroyRef);
  private readonly cameraControlsService: CameraControlsService = inject(CameraControlsService);
  private readonly colorService: ColorService = inject(ColorService);

  @ViewChild('canvas') public canvas: ElementRef = null!;

  private ambientLightLowIntensity = 2.0;
  public readonly rawModel = input<ArrayBuffer>();
  public readonly lineForEdit = input<LineDto | undefined>();
  public readonly revertLastPointCommand = input(0);
  public shortcuts: ShortcutInput[] = [];

  private readonly processedRawModel = signal<ArrayBuffer | undefined>(undefined);

  private readonly scene: THREE.Scene = new THREE.Scene();
  private readonly loader: GLTFLoader = new GLTFLoader();
  private readonly ambientLightIntensity: number = 2.0;
  private readonly directionalLightIntensity: number = 1.0;
  private readonly ambientLight: THREE.AmbientLight = new THREE.AmbientLight(0xffffff, this.ambientLightIntensity);
  private readonly directionalLight: THREE.DirectionalLight = new THREE.DirectionalLight(
    0xffffff,
    this.directionalLightIntensity
  );
  private readonly raycaster: THREE.Raycaster = new THREE.Raycaster();
  private readonly mouseHelper: THREE.Mesh = new THREE.Mesh(
    new THREE.BoxGeometry(1, 1, 10),
    new THREE.MeshNormalMaterial()
  );
  private readonly lineGeometry: THREE.BufferGeometry = new THREE.BufferGeometry();
  private readonly line: THREE.Line = new THREE.Line(this.lineGeometry, new THREE.LineBasicMaterial());
  private readonly debugColor: number = 0x98ff98;
  private readonly debugSphere: THREE.Mesh = new THREE.Mesh(
    new THREE.SphereGeometry(1, 32, 32),
    new THREE.MeshBasicMaterial({ color: this.debugColor })
  );
  private readonly viewpoints: Record<string, Viewpoint> = {
    seitensprung: {
      position: new THREE.Vector3(-9.44876021135404, 7.320794224154875, 6.724980613386679),
      target: new THREE.Vector3(-3.0950377146341763, 7.692263096560984, -2.217579333053568)
    },
    overview: {
      position: new THREE.Vector3(-0.0967487267161844, 9.600426820701172, 11.401826642615301),
      target: new THREE.Vector3(3.710864794256258, 5.128171870749298, -1.8769139834889017)
    }
  };
  private readonly position: THREE.Vector3 = new THREE.Vector3();
  private readonly orientation: THREE.Euler = new THREE.Euler();
  private readonly loggedPoints: LoggedPoint[] = [];
  private readonly sphereArray: THREE.Mesh[] = [];
  private readonly currentMeshes: THREE.Mesh[] = [];
  private readonly currentIntersections: THREE.Intersection<THREE.Object3D<THREE.Object3DEventMap>>[] = [];
  private readonly tubeParams = {
    radius: 0.05,
    extrusionSegments: 100,
    radiusSegments: 6
  };
  private readonly pointParams = {
    radius: 0.1
  };
  private readonly tubeMaterial: THREE.MeshBasicMaterial = new THREE.MeshBasicMaterial({
    color: this.colorService.nextColor(),
    transparent: true,
    opacity: 0.3,
    depthTest: false,
    depthWrite: false
  });
  private readonly rayVisionMaterial: THREE.MeshStandardMaterial = new THREE.MeshStandardMaterial({
    color: this.tubeMaterial.color
  });
  private camera: THREE.PerspectiveCamera = null!;
  private controls: OrbitControls = null!;
  private renderer: THREE.WebGLRenderer = null!;
  private dragControls?: DragControls;
  private tubeGeometry?: THREE.TubeGeometry;
  private tubeMesh?: THREE.Mesh;
  private rayVisionTubeMesh?: THREE.Mesh;
  private hitMesh?: THREE.Mesh;
  private currentGltf?: GLTF;
  private isDragging = false;
  private isLooping = false;
  private displayNormals = false;
  private displayWireframe = false;
  private initialized = false;

  private readonly intersection = {
    intersects: false,
    point: new THREE.Vector3(),
    normal: new THREE.Vector3()
  };
  // debugging stuff end

  // Shader material related
  private originalBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockTexture: THREE.Texture | null = null;

  private bvh: GeometryBVH | undefined;
  // private bvhHelper = new BVHHelper();
  public onResize(): void {
    if (this.renderer) {
      const canvasSizes: { width: number; height: number } = {
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

  public constructor() {
    effect(() => {
      const rawModel = this.rawModel();
      // if (!this.initialized) {
      //   return;
      // }
      if (rawModel !== this.proccessedRawModel()) {
        this.proccessedRawModel.set(rawModel);
        if (rawModel !== undefined) {
          // this effect can run through before afterInit is finished. Needs fixing.
          this.removePreviousAndAddBoulderToScene(rawModel);
        }
      }
    });

    effect(() => {
      const line: LineDto | undefined = this.lineForEdit();
      this.clearLoggedPoints();

      if (line === undefined || line.data?.positions === undefined) {
        return;
      }

      for (const positions of line.data?.positions ?? []) {
        const position: THREE.Vector3 = new THREE.Vector3(positions[0], positions[1], positions[2]);
        const loggedPoint: LoggedPoint = { id: crypto.randomUUID(), position };
        this.loggedPoints.push(loggedPoint);
        this.generatePoint(loggedPoint.id, loggedPoint.position);
      }

      if (this.initialized) {
        this.regeneratePath();
      }
    });

    effect(() => {
      const commandValue: number = this.revertLastPointCommand();
      if (commandValue === 0) {
        return;
      }

      this.removeLastPoint();
    });

    this.destroyRef.onDestroy(() => this.dispose());

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

    this.initialized = true;
    this.resetCameraPosition();
  }

  public getLinePoints(): number[][] | undefined {
    if (this.loggedPoints.length < 3) {
      return undefined;
    }

    const linePoints = this.loggedPoints.map((lp) => [lp.position.x, lp.position.y, lp.position.z]);
    return linePoints;
  }

  public removeLastPoint(): void {
    this.loggedPoints.pop();
    const sphere: THREE.Mesh | undefined = this.sphereArray.pop();
    if (sphere) {
      this.scene.remove(sphere);
    }
    this.regeneratePath();
  }

    if (!this.renderer || !this.camera || !this.raycaster) {
  private onPointerMove = (event: PointerEvent): void => {
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

    if (!this.canvas.nativeElement.contains(event.target as Node)) {
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
    const sphere: THREE.Mesh = new THREE.Mesh(
      new THREE.SphereGeometry(this.pointParams.radius, 16, 16),
      new THREE.MeshNormalMaterial()
    );
    sphere.uuid = uuid;

    if (this.loggedPoints.length > 0) {
      sphere.position.copy(point);
      this.sphereArray.push(sphere);
      this.scene.add(sphere);
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

    if (this.currentMeshes.length === 0 || this.raycaster === undefined || this.camera === undefined) {
  private checkIntersection(x: number, y: number): void {
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
    this.raycaster.firstHitOnly = true;

    this.currentIntersections.length = 0;

    const hit = this.raycaster.intersectObjects(this.currentMeshes, false);

    if (hit.length > 0) {
      const currentIntersection: THREE.Intersection<THREE.Object3D<THREE.Object3DEventMap>> = hit[0];
      this.hitMesh = currentIntersection.object as THREE.Mesh;

      const point: THREE.Vector3 = currentIntersection.point;
      this.mouseHelper.position.copy(point);
      this.intersection.point.copy(point);

      const normalMatrix: THREE.Matrix3 = new THREE.Matrix3().getNormalMatrix(this.hitMesh.matrixWorld);

      const normal = currentIntersection.face!.normal.clone();
      normal.applyNormalMatrix(normalMatrix);
      normal.multiplyScalar(10);
      normal.add(point);

      this.intersection.normal.copy(currentIntersection.face!.normal);
      this.mouseHelper.lookAt(normal);
      const lineLength = 1.0;
      const end = point.clone().addScaledVector(normal, lineLength);

      this.line.visible = true;
      const positions: THREE.BufferAttribute = this.line.geometry.attributes['position'] as THREE.BufferAttribute;
      positions.setXYZ(0, point.x, point.y, point.z);
      positions.setXYZ(1, end.x, end.y, end.z);
      positions.needsUpdate = true;

      this.intersection.intersects = true;
      this.currentIntersections.length = 0;
    } else {
      this.intersection.intersects = false;
      this.line.visible = false;
    }
  }

  private createCanvas(): void {
    const canvas: HTMLCanvasElement = this.canvas.nativeElement;
    if (!canvas) {
      return;
    }

    const canvasSizes: { width: number; height: number } = {
      width: canvas.offsetWidth,
      height: canvas.offsetHeight
    };

    this.renderer = new THREE.WebGLRenderer({
      canvas: canvas,
      alpha: true
    });
    this.renderer.setClearColor(0x000000, 0);

    this.camera = new THREE.PerspectiveCamera(75, canvasSizes.width / canvasSizes.height, 0.001, 250);
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
    this.controls.addEventListener('change', this.startLooping);

    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(1);

    this.scene.add(this.mouseHelper);
    this.lineGeometry.setFromPoints([new THREE.Vector3(), new THREE.Vector3()]);
    this.scene.add(this.line);
    this.scene.add(this.debugSphere);

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

    if (this.loggedPoints.length > 0) {
      this.regeneratePath();
    }

    this.startLooping();
  }

  private loop = () => {
    if (!this.renderer || !this.isLooping) {
      return;
    }

    this.isLooping = false;

    if (this.controls) {
      this.debugSphere.position.copy(this.controls.target);
    }

    if (this.cameraControlsService.needsAnimation) {
      this.cameraControlsService.animateTransition();
      this.isLooping = true; // keep looping until the animation is finished
    }

    this.renderer.render(this.scene, this.camera);
    if (this.isLooping) {
      window.requestAnimationFrame(this.loop);
    }
  };

  private removePreviousAndAddBoulderToScene(buffer: ArrayBuffer): void {
    this.loader.parse(
      buffer,
      '',
      (gltf: GLTF) => {
        const isFirstModel: boolean = this.currentGltf === undefined;

        if (this.currentGltf !== undefined) {
          this.removeBoulderFromScene(this.currentGltf);
        }

        this.scene.add(gltf.scene);
        gltf.scene.traverse((child) => {
          child.layers.set(1);
          const mesh: THREE.Mesh = child as THREE.Mesh;

          if (mesh.isMesh) {
            if (this.displayNormals) {
              const normalsMesh: VertexNormalsHelper = new VertexNormalsHelper(mesh, 0.5, this.debugColor);
              this.scene.add(normalsMesh);
            }

            mesh.geometry.computeBoundsTree();
            this.bvh = mesh.geometry.boundsTree;

            const material = mesh.material as THREE.MeshPhysicalMaterial;
            material.side = THREE.DoubleSide;
            if (this.displayWireframe) {
              material.wireframe = true;
            }
            if (!this.originalBlockMaterial) {
              this.originalBlockMaterial = material;
              this.originalBlockTexture = material.map;
            }
            material.needsUpdate = true;
            this.currentMeshes.push(mesh);
          }
        });

        this.currentGltf = gltf;

        if (isFirstModel) {
          this.resetCameraPosition();
        }

        this.raycaster = new THREE.Raycaster(this.camera.position);
        this.raycaster.layers.set(1);
        this.raycaster.firstHitOnly = true;

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
    this.bvh = undefined;
    this.currentMeshes.length = 0;
    this.hitMesh = undefined;
    this.originalBlockMaterial = undefined;
    gltf.scene.traverse((child) => {
      const mesh: THREE.Mesh = child as THREE.Mesh;
      if (mesh.isMesh) {
        mesh.geometry?.disposeBoundsTree();
        mesh.geometry?.dispose();
      }
    });
    this.scene.remove(gltf.scene);
  }

  private dispose(): void {
    window.removeEventListener('pointermove', this.onPointerMove);
    window.removeEventListener('pointerdown', this.addPointToLoggedPoints);

    this.controls?.removeEventListener('change', this.startLooping);
    this.controls?.dispose();

    if (this.currentGltf) {
      this.removeBoulderFromScene(this.currentGltf);
    }

    this.scene.traverse((child) => {
      const mesh: THREE.Mesh = child as THREE.Mesh;
      if (mesh.isMesh) {
        mesh.geometry?.dispose();
      }
    });

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

  private clearLoggedPoints(): void {
    this.loggedPoints.length = 0;

    for (const sphere of this.sphereArray) {
      this.scene.remove(sphere);
      sphere.geometry.dispose();
      const material = sphere.material;
      if (Array.isArray(material)) {
        for (const mat of material) {
          mat.dispose();
        }
      } else {
        material.dispose();
      }
    }
    this.sphereArray.length = 0;
  }
}

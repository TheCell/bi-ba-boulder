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
import { TransformControls } from 'three/examples/jsm/controls/TransformControls.js';
import { CameraControlsService } from '../camera-controls.service';
import { fitCameraToCenteredObject } from '../common/camera-utils';
import { ColorService } from '../../core/util-services/color.service';
import { Viewpoint } from '../common/viewpoint';
import { VertexNormalsHelper } from 'three/addons/helpers/VertexNormalsHelper.js';
import { DragControls } from 'three/addons/controls/DragControls.js';
import { LineDto, SceneMarking } from '@api-net/model/models';
import {
  OutdoorBlocMarkingsType,
  resolveHelperColor,
  resolveHelperTypeFromColor
} from '../common/outdoor-bloc-markings-types';
import {
  computeBoundsTree,
  disposeBoundsTree,
  acceleratedRaycast,
  disposeBatchedBoundsTree,
  computeBatchedBoundsTree
} from 'three-mesh-bvh';
import {
  boxPlacementDepth,
  boxPlacementHeight,
  boxPlacementWidth,
  fragmentShader,
  helperLayer,
  maxBoxMarkings,
  maxSphereMarkings,
  spherePlacementRadius,
  uniforms
} from '../common/outdoor-shader-code';
import { SceneMarkingForm } from '../../core/enums/scene-marking-form.enum';
import {
  BoxSceneMarking,
  CustomSceneMarking,
  HelperShaderSnapshot,
  MaterialShader,
  SphereSceneMarking
} from '../outdoor-interfaces/scene-marking';
import { createHelperOverlayTexture } from '../common/outdoor-bloc-utils';
import { RawModelInput } from '../outdoor-renderer/model-input.interface';
import { RESOLUTION_LEVEL, ResolutionLevel } from '../../interfaces/resolution-level';

THREE.BufferGeometry.prototype.computeBoundsTree = computeBoundsTree;
THREE.BufferGeometry.prototype.disposeBoundsTree = disposeBoundsTree;
THREE.Mesh.prototype.raycast = acceleratedRaycast;

THREE.BatchedMesh.prototype.computeBoundsTree = computeBatchedBoundsTree;
THREE.BatchedMesh.prototype.disposeBoundsTree = disposeBatchedBoundsTree;
THREE.BatchedMesh.prototype.raycast = acceleratedRaycast;

export type InteractionMode = 'line' | 'sphere-marking' | 'box-marking' | 'select-helper';
export type HelperTransformMode = 'translate' | 'rotate' | 'scale';

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

  public rawModels = input<RawModelInput[]>([]);
  public readonly lineForEdit = input<LineDto | undefined>();
  public readonly interactionModeSelection = input<InteractionMode>('line');
  public readonly blocMarkingsTypeSelection = input<OutdoorBlocMarkingsType>(OutdoorBlocMarkingsType.start);
  public readonly transformModeSelection = input<HelperTransformMode>('rotate');
  public readonly revertLastPointCommand = input(0);
  public shortcuts: ShortcutInput[] = [];

  private proccessedRawModels = signal<RawModelInput[]>([]);
  private readonly interactionMode = signal<InteractionMode>('line');
  private readonly blocMarkingsType = signal<OutdoorBlocMarkingsType>(OutdoorBlocMarkingsType.start);
  private readonly helperTransformMode = signal<HelperTransformMode>('rotate');

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
  private readonly helperRaycaster: THREE.Raycaster = new THREE.Raycaster();
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
  private readonly helperObjects: CustomSceneMarking[] = [];
  private readonly currentMeshes: THREE.Mesh[] = [];
  private readonly sphereMarkingData: THREE.Vector4[] = Array.from(
    { length: maxSphereMarkings },
    () => new THREE.Vector4(0, 0, 0, 0)
  );
  private readonly sphereMarkingColors: THREE.Color[] = Array.from(
    { length: maxSphereMarkings },
    () => new THREE.Color(0x000000)
  );
  private readonly boxMarkingPositions: THREE.Vector3[] = Array.from(
    { length: maxBoxMarkings },
    () => new THREE.Vector3(0, 0, 0)
  );
  private readonly boxMarkingQuaternions: THREE.Vector4[] = Array.from(
    { length: maxBoxMarkings },
    () => new THREE.Vector4(0, 0, 0, 1)
  );
  private readonly boxMarkingSizes: THREE.Vector3[] = Array.from(
    { length: maxBoxMarkings },
    () => new THREE.Vector3(0, 0, 0)
  );
  private readonly boxMarkingColors: THREE.Color[] = Array.from(
    { length: maxBoxMarkings },
    () => new THREE.Color(0x000000)
  );
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
  private readonly helperOverlayTexture: THREE.DataTexture = createHelperOverlayTexture();

  private camera: THREE.PerspectiveCamera = null!;
  private controls: OrbitControls = null!;
  private renderer: THREE.WebGLRenderer = null!;
  private transformControls?: TransformControls;
  private dragControls?: DragControls;
  private tubeGeometry?: THREE.TubeGeometry;
  private tubeMesh?: THREE.Mesh;
  private rayVisionTubeMesh?: THREE.Mesh;
  private hitMesh?: THREE.Mesh;
  private helperShaderMaterials: THREE.MeshPhysicalMaterial[] = [];
  private selectedHelper?: CustomSceneMarking;
  private isDragging = false;
  private isLooping = false;
  private displayNormals = false;
  private displayWireframe = false;

  private sceneObjects: { lod: THREE.LOD; blocId: string }[] = [];
  private initialized = false;

  private readonly intersection = {
    intersects: false,
    point: new THREE.Vector3(),
    normal: new THREE.Vector3()
  };
  private frameRequested = false;

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
      const rawModels = this.rawModels();
      const proccessedRawModels = this.proccessedRawModels();
      // if (rawModels !== proccessedRawModels) {
      const setCameraUp = this.proccessedRawModels().length === 0;

      for (const rawModel of rawModels) {
        const alreadyProcessed = proccessedRawModels.find(
          (processed) => processed.blocId === rawModel.blocId && processed.resolution === rawModel.resolution
        );
        if (!alreadyProcessed) {
          this.addBlocOrLodToScene(rawModel.arrayBuffer, rawModel.resolution, rawModel.blocId, setCameraUp);
          this.proccessedRawModels.set([...proccessedRawModels, rawModel]);
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

      if (line.data.sceneMarkings !== undefined) {
        this.addMarkingsForEdit(line.data.sceneMarkings);
      }

      if (this.initialized) {
        this.regeneratePath();

        this.updateMarkingShaderUniforms();
        this.applyShaderUniformUpdates();
        this.startLooping();
      }
    });

    effect(() => {
      const commandValue: number = this.revertLastPointCommand();
      if (commandValue === 0) {
        return;
      }

      this.removeLastPoint();
    });

    effect(() => {
      const mode: InteractionMode = this.interactionModeSelection();
      if (mode !== this.interactionMode()) {
        this.setInteractionMode(mode);
      }
    });

    effect(() => {
      const selectedMarkingType: OutdoorBlocMarkingsType = this.blocMarkingsTypeSelection();
      if (selectedMarkingType !== this.blocMarkingsType()) {
        this.blocMarkingsType.set(selectedMarkingType);
      }
    });

    effect(() => {
      const mode: HelperTransformMode = this.transformModeSelection();
      if (mode !== this.helperTransformMode()) {
        this.setTransformMode(mode);
      }
    });

    this.destroyRef.onDestroy(() => this.dispose());

    this.mouseHelper.visible = false;
    window.addEventListener('pointermove', this.onPointerMove);

    this.shortcuts.push(
      {
        key: ['ctrl + z'],
        preventDefault: true,
        command: (_: ShortcutEventOutput) => this.removeLastPoint()
      },
      {
        key: ['delete', 'backspace'],
        preventDefault: true,
        command: (_: ShortcutEventOutput) => this.removeSelectedHelper()
      },
      {
        key: ['escape'],
        preventDefault: true,
        command: (_: ShortcutEventOutput) => this.clearSelectedHelper()
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

  public getSceneMarkings(): SceneMarking[] {
    const markings: SceneMarking[] = [];

    let sphereIndex = 0;
    let boxIndex = 0;
    for (const helper of this.helperObjects) {
      if (helper.type === 'sphere' && sphereIndex < maxSphereMarkings) {
        markings.push({
          form: SceneMarkingForm.Sphere,
          position: this.sphereMarkingData[sphereIndex].toArray().slice(0, 3) as [number, number, number],
          quaternion: [0, 0, 0, 1],
          scale: [helper.mesh.scale.x, helper.mesh.scale.y, helper.mesh.scale.z],
          type: resolveHelperTypeFromColor(helper.color)
        });
        sphereIndex++;
        continue;
      }

      if (helper.type === 'box' && boxIndex < maxBoxMarkings) {
        markings.push({
          form: SceneMarkingForm.Box,
          position: this.boxMarkingPositions[boxIndex].toArray() as [number, number, number],
          quaternion: this.boxMarkingQuaternions[boxIndex].toArray() as [number, number, number, number],
          scale: this.boxMarkingSizes[boxIndex].toArray() as [number, number, number],
          type: resolveHelperTypeFromColor(helper.color)
        });
        boxIndex++;
      }
    }

    return markings;
  }

  public removeLastPoint(): void {
    this.loggedPoints.pop();
    const sphere: THREE.Mesh | undefined = this.sphereArray.pop();
    if (sphere) {
      this.scene.remove(sphere);
    }
    this.regeneratePath();
  }

  private onPointerMove = (event: PointerEvent): void => {
    if (!this.renderer || !this.camera) {
      return;
    }

    if (this.isDragging || !this.canvas.nativeElement.contains(event.target as Node)) {
      return;
    }

    this.checkIntersection(event.clientX, event.clientY);
    this.startLooping();
  };

  private handleCanvasPointerDown = (event: PointerEvent): void => {
    if (event.button === 2 || this.transformControls?.dragging) {
      return;
    }

    if (!this.canvas.nativeElement.contains(event.target as Node)) {
      return;
    }

    const hitHelper: CustomSceneMarking | undefined = this.getHelperAtPointer(event.clientX, event.clientY);
    if (hitHelper) {
      this.selectHelper(hitHelper);
      return;
    }

    if (!this.intersection.intersects) {
      if (this.interactionMode() === 'select-helper') {
        this.clearSelectedHelper();
      }

      return;
    }

    this.position.copy(this.intersection.point);
    this.orientation.copy(this.mouseHelper.rotation);

    switch (this.interactionMode()) {
      case 'line':
        this.addLinePoint();
        return;
      case 'sphere-marking':
        this.addSphereMarking();
        return;
      case 'box-marking':
        this.addBoxMarking();
        return;
      case 'select-helper':
        this.clearSelectedHelper();
        return;
    }
  };

  private addMarkingsForEdit(sceneMarkings: SceneMarking[]) {
    for (const marking of sceneMarkings) {
      switch (marking.form) {
        case SceneMarkingForm.Sphere:
          this.addSphereMarking(marking);
          break;
        case SceneMarkingForm.Box:
          this.addBoxMarking(marking);
          break;
        default:
          throw new Error(`Unsupported scene marking form: ${marking.form}`);
      }
    }
  }

  private addLinePoint(): void {
    const loggedPoint: LoggedPoint = { id: crypto.randomUUID(), position: this.position.clone() };
    this.loggedPoints.push(loggedPoint);
    this.generatePoint(loggedPoint.id, loggedPoint.position);
    this.regeneratePath();
  }

  private addSphereMarking(marking?: SceneMarking): void {
    if (
      marking !== undefined &&
      (marking?.type === undefined ||
        marking?.scale === undefined ||
        marking?.quaternion === undefined ||
        marking?.position === undefined)
    ) {
      throw new Error('Scene marking is missing required properties: type, scale, quaternion, or position');
    }

    let type = this.blocMarkingsType();
    if (marking?.type !== undefined) {
      type = marking.type;
    }
    const color: THREE.Color = resolveHelperColor(type);
    const mesh: THREE.Mesh = new THREE.Mesh(
      new THREE.SphereGeometry(1, 24, 24),
      new THREE.MeshStandardMaterial({
        color,
        transparent: true,
        opacity: 0.75,
        depthWrite: false
      })
    );

    if (marking === undefined || marking.position === undefined) {
      mesh.position.copy(this.position);
    } else {
      mesh.position.set(marking.position[0], marking.position[1], marking.position[2]);
    }

    let scale = spherePlacementRadius;
    if (marking !== undefined && marking.scale !== undefined) {
      scale = marking.scale[0];
    }
    mesh.scale.setScalar(scale);
    mesh.layers.set(helperLayer);

    const helper: SphereSceneMarking = {
      id: crypto.randomUUID(),
      color,
      mesh,
      type: 'sphere'
    };

    mesh.userData['helperId'] = helper.id;
    mesh.userData['helperType'] = helper.type;
    this.helperObjects.push(helper);
    this.scene.add(mesh);
    this.selectHelper(helper);
    this.updateMarkingShaderUniforms();
    this.applyShaderUniformUpdates();
    this.startLooping();
  }

  private addBoxMarking(marking?: SceneMarking): void {
    if (
      marking !== undefined &&
      (marking?.type === undefined ||
        marking?.scale === undefined ||
        marking?.quaternion === undefined ||
        marking?.position === undefined)
    ) {
      throw new Error('Scene marking is missing required properties: type, scale, quaternion, or position');
    }

    let type = this.blocMarkingsType();
    if (marking?.type !== undefined) {
      type = marking.type;
    }
    const color: THREE.Color = resolveHelperColor(type);
    const mesh: THREE.Mesh = new THREE.Mesh(
      new THREE.BoxGeometry(1, 1, 1),
      new THREE.MeshStandardMaterial({
        color,
        transparent: true,
        opacity: 0.45,
        depthWrite: false
      })
    );

    const helperNormal: THREE.Vector3 = this.intersection.normal.clone().normalize();
    let orientation: THREE.Quaternion = new THREE.Quaternion().setFromUnitVectors(
      new THREE.Vector3(0, 0, 1),
      helperNormal
    );
    if (marking?.quaternion !== undefined) {
      orientation = new THREE.Quaternion(
        marking.quaternion[0],
        marking.quaternion[1],
        marking.quaternion[2],
        marking.quaternion[3]
      );
    }

    if (marking === undefined || marking.position === undefined) {
      mesh.position.copy(this.position);
    } else {
      mesh.position.set(marking.position[0], marking.position[1], marking.position[2]);
    }
    mesh.quaternion.copy(orientation);

    let scale = new THREE.Vector3(boxPlacementWidth, boxPlacementHeight, boxPlacementDepth);
    if (marking !== undefined && marking.scale !== undefined) {
      scale = new THREE.Vector3(marking.scale[0], marking.scale[1], marking.scale[2]);
    }
    mesh.scale.copy(scale);
    mesh.layers.set(helperLayer);

    const helper: BoxSceneMarking = {
      id: crypto.randomUUID(),
      color,
      mesh,
      type: 'box'
    };

    mesh.userData['helperId'] = helper.id;
    mesh.userData['helperType'] = helper.type;
    this.helperObjects.push(helper);
    this.scene.add(mesh);
    if (marking === undefined) {
      this.selectHelper(helper);
    }
    this.updateMarkingShaderUniforms();
    this.applyShaderUniformUpdates();
    this.startLooping();
  }

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

  private checkIntersection(x: number, y: number): void {
    if (this.currentMeshes.length === 0 || this.camera === undefined) {
      return;
    }

    const pointer: THREE.Vector2 = this.getCanvasPointer(x, y);

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
      const worldNormal: THREE.Vector3 =
        currentIntersection.face?.normal.clone().applyNormalMatrix(normalMatrix).normalize() ??
        new THREE.Vector3(0, 0, 1);
      const normalEnd: THREE.Vector3 = point.clone().addScaledVector(worldNormal, 1.0);

      this.intersection.normal.copy(worldNormal);
      this.mouseHelper.lookAt(normalEnd);

      this.line.visible = true;
      const positions: THREE.BufferAttribute = this.line.geometry.attributes['position'] as THREE.BufferAttribute;
      positions.setXYZ(0, point.x, point.y, point.z);
      positions.setXYZ(1, normalEnd.x, normalEnd.y, normalEnd.z);
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
    this.camera.layers.enable(helperLayer);

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

    this.raycaster.layers.set(1);
    this.helperRaycaster.layers.set(helperLayer);

    this.scene.add(this.mouseHelper);
    this.lineGeometry.setFromPoints([new THREE.Vector3(), new THREE.Vector3()]);
    this.scene.add(this.line);
    this.scene.add(this.debugSphere);

    this.transformControls = new TransformControls(this.camera, this.renderer.domElement);
    this.transformControls.setMode(this.helperTransformMode());
    this.transformControls.addEventListener('dragging-changed', this.onTransformDraggingChanged);
    this.transformControls.addEventListener('objectChange', this.onTransformObjectChange);
    this.scene.add(this.transformControls.getHelper());

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

    canvas.addEventListener('pointerdown', this.handleCanvasPointerDown);

    if (this.loggedPoints.length > 0) {
      this.regeneratePath();
    }

    this.startLooping();
  }

  private loop = () => {
    this.frameRequested = false;

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
      this.requestNextFrame();
    }
  };

  private addBlocOrLodToScene(
    arrayBuffer: ArrayBuffer,
    resolution: ResolutionLevel,
    blocId: string,
    resetCamera: boolean
  ): void {
    this.loader.parse(
      arrayBuffer,
      '',
      (gltf: GLTF) => {
        gltf.scene.traverse((child) => {
          child.layers.set(1);
          const mesh = child as THREE.Mesh;
          if (mesh.isMesh) {
            mesh.material = this.createHelperShaderMaterial(mesh.material);

            if (this.displayNormals) {
              const normalsMesh: VertexNormalsHelper = new VertexNormalsHelper(mesh, 0.5, this.debugColor);
              this.scene.add(normalsMesh);
            }

            mesh.geometry.computeBoundsTree();

            const material: THREE.Material | THREE.Material[] = mesh.material;
            if (!Array.isArray(material) && material instanceof THREE.MeshPhysicalMaterial) {
              material.side = THREE.DoubleSide; // important for raycasting
              if (this.displayWireframe) {
                material.wireframe = true;
              }
            }

            mesh.material = this.createHelperShaderMaterial(mesh.material);
            this.currentMeshes.push(mesh);
          }
        });

        const object = this.sceneObjects.find((sceneObject) => sceneObject.blocId === blocId);
        if (object !== undefined) {
          object.lod.addLevel(gltf.scene, this.getLodDistanceForResolution(resolution));
          // this.scene.add(object.lod);
        } else {
          const lod = new THREE.LOD();
          lod.addLevel(gltf.scene, this.getLodDistanceForResolution(resolution));
          this.sceneObjects.push({ lod: lod, blocId });
          this.scene.add(lod);
        }

        if (resetCamera) {
          this.resetCameraPosition();
        }
        this.loop();
        window.requestAnimationFrame(() => {
          this.applyShaderUniformUpdates();
          this.loop();
        });
      },
      (err: ErrorEvent) => {
        throw new Error('Error loading GLTF: ' + err.message);
      }
    );
  }

  // private removePreviousAndAddBoulderToScene(buffer: ArrayBuffer): void {
  //   this.loader.parse(
  //     buffer,
  //     '',
  //     (gltf: GLTF) => {
  //       const isFirstModel: boolean = this.currentGltf === undefined;

  //       if (this.currentGltf !== undefined) {
  //         this.removeBoulderFromScene(this.currentGltf);
  //       }

  //       this.scene.add(gltf.scene);
  //       gltf.scene.traverse((child) => {
  //         child.layers.set(1);
  //         const mesh: THREE.Mesh = child as THREE.Mesh;

  //         if (mesh.isMesh) {
  //           if (this.displayNormals) {
  //             const normalsMesh: VertexNormalsHelper = new VertexNormalsHelper(mesh, 0.5, this.debugColor);
  //             this.scene.add(normalsMesh);
  //           }

  //           mesh.geometry.computeBoundsTree();

  //           const material: THREE.Material | THREE.Material[] = mesh.material;
  //           if (!Array.isArray(material) && material instanceof THREE.MeshPhysicalMaterial) {
  //             material.side = THREE.DoubleSide; // important for raycasting
  //             if (this.displayWireframe) {
  //               material.wireframe = true;
  //             }
  //           }

  //           mesh.material = this.createHelperShaderMaterial(mesh.material);
  //           this.currentMeshes.push(mesh);
  //         }
  //       });

  //       this.currentGltf = gltf;

  //       if (isFirstModel) {
  //         this.resetCameraPosition();
  //       }

  //       this.raycaster.layers.set(1);
  //       this.raycaster.firstHitOnly = true;
  //       this.updateMarkingShaderUniforms();
  //       this.applyShaderUniformUpdates();

  //       this.startLooping();

  //       // Re-apply uniforms after first render when shaders have compiled
  //       window.requestAnimationFrame(() => {
  //         this.applyShaderUniformUpdates();
  //         this.startLooping();
  //       });
  //     },
  //     (err: ErrorEvent) => {
  //       throw new Error(err.message);
  //     }
  //   );
  // }

  // private resetCameraPosition(): void {
  //   if (this.initialized && this.currentGltf) {
  //     fitCameraToCenteredObject(this.camera, this.currentGltf.scene, 0, this.controls);
  //     this.cameraControlsService.setOrbitControls(this.controls);
  //   }
  // }

  private resetCameraPosition(): void {
    if (this.initialized && this.sceneObjects.length > 0) {
      const lod = this.sceneObjects[0].lod;
      let mainMesh: THREE.Mesh | undefined = undefined;
      lod.traverse((object) => {
        if (mainMesh !== undefined) {
          return;
        }

        if (object instanceof THREE.Mesh && !mainMesh) {
          mainMesh = object;
        }
      });

      const model = mainMesh ?? lod.children[0];
      fitCameraToCenteredObject(this.camera, model, 0, this.controls);

      this.cameraControlsService.setOrbitControls(this.controls);
    }
  }

  private removeBoulderFromScene(gltf: GLTF): void {
    this.currentMeshes.length = 0;
    this.hitMesh = undefined;
    for (const helperShaderMaterial of this.helperShaderMaterials) {
      helperShaderMaterial.dispose();
    }
    this.helperShaderMaterials.length = 0;

    gltf.scene.traverse((child) => {
      const mesh: THREE.Mesh = child as THREE.Mesh;
      if (mesh.isMesh) {
        mesh.geometry?.disposeBoundsTree();
        mesh.geometry?.dispose();
      }
    });
    this.scene.remove(gltf.scene);
  }

  private createHelperShaderMaterial(
    originalMaterial: THREE.Material | THREE.Material[]
  ): THREE.Material | THREE.Material[] {
    if (Array.isArray(originalMaterial)) {
      return originalMaterial.map((material) => this.createHelperShaderMaterialForSingleMaterial(material));
    }

    return this.createHelperShaderMaterialForSingleMaterial(originalMaterial);
  }

  private createHelperShaderMaterialForSingleMaterial(originalMaterial: THREE.Material): THREE.Material {
    if (!(originalMaterial instanceof THREE.MeshPhysicalMaterial)) {
      return originalMaterial;
    }

    const material: THREE.MeshPhysicalMaterial = originalMaterial.clone();
    material.side = THREE.DoubleSide; // important for raycasting
    material.wireframe = this.displayWireframe;
    material.needsUpdate = true;
    this.helperShaderMaterials.push(material);

    material.onBeforeCompile = (shader: MaterialShader): void => {
      shader.uniforms['helperOverlayTexture'] = { value: this.helperOverlayTexture };
      shader.uniforms['sphereMarkingCount'] = { value: 0 };
      shader.uniforms['sphereMarkings'] = { value: this.sphereMarkingData };
      shader.uniforms['sphereMarkingColors'] = { value: this.sphereMarkingColors };
      shader.uniforms['boxMarkingCount'] = { value: 0 };
      shader.uniforms['boxMarkingPositions'] = { value: this.boxMarkingPositions };
      shader.uniforms['boxMarkingQuaternions'] = { value: this.boxMarkingQuaternions };
      shader.uniforms['boxMarkingSizes'] = { value: this.boxMarkingSizes };
      shader.uniforms['boxMarkingColors'] = { value: this.boxMarkingColors };
      shader.uniforms['helperBlendStrength'] = { value: 0.65 };
      shader.uniforms['helperEmissiveStrength'] = { value: 0.9 };
      shader.uniforms['sphereFalloff'] = { value: 0.45 };
      shader.uniforms['boxEdgeFalloff'] = { value: 0.3 };
      shader.uniforms['maxSphereMarkings'] = { value: maxSphereMarkings };
      shader.uniforms['maxBoxMarkings'] = { value: maxBoxMarkings };

      shader.vertexShader = shader.vertexShader.replace(
        'varying vec3 vViewPosition;',
        ['varying vec3 vViewPosition;', 'varying vec3 vWorldPosition;'].join('\n')
      );

      shader.vertexShader = shader.vertexShader.replace(
        '#include <worldpos_vertex>',
        ['#include <worldpos_vertex>', 'vWorldPosition = (modelMatrix * vec4(transformed, 1.0)).xyz;'].join('\n')
      );

      shader.fragmentShader = shader.fragmentShader.replace(
        'uniform float opacity;',
        uniforms(maxSphereMarkings, maxBoxMarkings).join('\n')
      );

      shader.fragmentShader = shader.fragmentShader.replace('#include <map_fragment>', fragmentShader.join('\n'));

      material.userData['shader'] = shader;
    };

    return material;
  }

  private applyShaderUniformUpdates(): void {
    const helperSnapshot: HelperShaderSnapshot = this.updateMarkingShaderUniforms();
    for (const helperShaderMaterial of this.helperShaderMaterials) {
      const shader: MaterialShader | undefined = helperShaderMaterial.userData['shader'] as MaterialShader | undefined;
      if (!shader) {
        continue;
      }

      shader.uniforms['helperOverlayTexture'].value = this.helperOverlayTexture;
      shader.uniforms['sphereMarkingCount'].value = helperSnapshot.sphereMarkingCount;
      shader.uniforms['sphereMarkings'].value = this.sphereMarkingData;
      shader.uniforms['sphereMarkingColors'].value = this.sphereMarkingColors;
      shader.uniforms['boxMarkingCount'].value = helperSnapshot.boxMarkingCount;
      shader.uniforms['boxMarkingPositions'].value = this.boxMarkingPositions;
      shader.uniforms['boxMarkingQuaternions'].value = this.boxMarkingQuaternions;
      shader.uniforms['boxMarkingSizes'].value = this.boxMarkingSizes;
      shader.uniforms['boxMarkingColors'].value = this.boxMarkingColors;
    }
  }

  private updateMarkingShaderUniforms(): HelperShaderSnapshot {
    let sphereIndex = 0;
    let boxIndex = 0;

    this.resetMarkingUniformArrays();

    for (const helper of this.helperObjects) {
      if (helper.type === 'sphere' && sphereIndex < maxSphereMarkings) {
        const radius: number = Math.max(helper.mesh.scale.x, 0.05);
        this.sphereMarkingData[sphereIndex].set(
          helper.mesh.position.x,
          helper.mesh.position.y,
          helper.mesh.position.z,
          radius
        );
        this.sphereMarkingColors[sphereIndex].copy(helper.color);
        sphereIndex++;
        continue;
      }

      if (helper.type === 'box' && boxIndex < maxBoxMarkings) {
        const normalizedQuaternion: THREE.Quaternion = helper.mesh.quaternion.clone().normalize();
        this.boxMarkingPositions[boxIndex].copy(helper.mesh.position);
        this.boxMarkingQuaternions[boxIndex].set(
          normalizedQuaternion.x,
          normalizedQuaternion.y,
          normalizedQuaternion.z,
          normalizedQuaternion.w
        );
        this.boxMarkingSizes[boxIndex].set(
          Math.max(Math.abs(helper.mesh.scale.x), 0.1),
          Math.max(Math.abs(helper.mesh.scale.y), 0.1),
          Math.max(Math.abs(helper.mesh.scale.z), 0.05)
        );
        this.boxMarkingColors[boxIndex].copy(helper.color);
        boxIndex++;
      }
    }

    return {
      sphereMarkingCount: sphereIndex,
      boxMarkingCount: boxIndex
    };
  }

  private resetMarkingUniformArrays(): void {
    for (let index = 0; index < maxSphereMarkings; index++) {
      this.sphereMarkingData[index].set(0, 0, 0, 0);
      this.sphereMarkingColors[index].setRGB(0, 0, 0);
    }

    for (let index = 0; index < maxBoxMarkings; index++) {
      this.boxMarkingPositions[index].set(0, 0, 0);
      this.boxMarkingQuaternions[index].set(0, 0, 0, 1);
      this.boxMarkingSizes[index].set(0, 0, 0);
      this.boxMarkingColors[index].setRGB(0, 0, 0);
    }
  }

  private getCanvasPointer(clientX: number, clientY: number): THREE.Vector2 {
    const canvasWidth: number = this.canvas.nativeElement.offsetWidth;
    const canvasHeight: number = this.canvas.nativeElement.offsetHeight;
    const canvasTop: number = this.canvas.nativeElement.getBoundingClientRect().top;
    const canvasLeft: number = this.canvas.nativeElement.getBoundingClientRect().left;
    const mouseX: number = clientX - canvasLeft;
    const mouseY: number = clientY - canvasTop;

    return new THREE.Vector2((mouseX / canvasWidth) * 2 - 1, -(mouseY / canvasHeight) * 2 + 1);
  }

  private getHelperAtPointer(clientX: number, clientY: number): CustomSceneMarking | undefined {
    if (this.helperObjects.length === 0) {
      return undefined;
    }

    const pointer: THREE.Vector2 = this.getCanvasPointer(clientX, clientY);
    this.helperRaycaster.setFromCamera(pointer, this.camera);
    const hits: THREE.Intersection<THREE.Object3D<THREE.Object3DEventMap>>[] = this.helperRaycaster.intersectObjects(
      this.helperObjects.map((helper) => helper.mesh),
      false
    );
    const helperId: string | undefined = hits[0]?.object.userData['helperId'] as string | undefined;

    if (!helperId) {
      return undefined;
    }

    return this.helperObjects.find((helper) => helper.id === helperId);
  }

  private selectHelper(helper: CustomSceneMarking): void {
    this.selectedHelper = helper;
    this.transformControls?.attach(helper.mesh);
    this.transformControls?.setMode(this.helperTransformMode());
    this.startLooping();
  }

  private clearSelectedHelper(): void {
    this.selectedHelper = undefined;
    this.transformControls?.detach();
    this.startLooping();
  }

  private setInteractionMode(mode: InteractionMode): void {
    this.interactionMode.set(mode);
    if (mode !== 'select-helper' && this.selectedHelper) {
      this.transformControls?.attach(this.selectedHelper.mesh);
    }
  }

  private setTransformMode(mode: HelperTransformMode): void {
    this.helperTransformMode.set(mode);
    this.transformControls?.setMode(mode);
    this.startLooping();
  }

  private removeSelectedHelper(): void {
    if (!this.selectedHelper) {
      return;
    }

    const selectedHelperId: string = this.selectedHelper.id;
    this.disposeHelperMesh(this.selectedHelper);
    this.clearSelectedHelper();

    const helperIndex: number = this.helperObjects.findIndex((helper) => helper.id === selectedHelperId);
    if (helperIndex >= 0) {
      this.helperObjects.splice(helperIndex, 1);
      this.updateMarkingShaderUniforms();
      this.applyShaderUniformUpdates();
      this.startLooping();
    }
  }

  private onTransformDraggingChanged = (event: { value: unknown }): void => {
    const isDragging: boolean = event.value === true;
    this.isDragging = isDragging;
    this.controls.enabled = !isDragging;
    this.cameraControlsService.setCameraInteractable(!isDragging);
  };

  private onTransformObjectChange = (): void => {
    if (!this.selectedHelper) {
      return;
    }

    if (this.selectedHelper.type === 'sphere') {
      const uniformScale: number = Math.max(
        (Math.abs(this.selectedHelper.mesh.scale.x) +
          Math.abs(this.selectedHelper.mesh.scale.y) +
          Math.abs(this.selectedHelper.mesh.scale.z)) /
          3,
        0.05
      );
      this.selectedHelper.mesh.scale.setScalar(uniformScale);
    } else {
      this.selectedHelper.mesh.scale.set(
        Math.max(Math.abs(this.selectedHelper.mesh.scale.x), 0.1),
        Math.max(Math.abs(this.selectedHelper.mesh.scale.y), 0.1),
        Math.max(Math.abs(this.selectedHelper.mesh.scale.z), 0.1)
      );
    }

    this.updateMarkingShaderUniforms();
    this.applyShaderUniformUpdates();
    this.startLooping();
  };

  private disposeHelperMesh(helper: CustomSceneMarking): void {
    this.scene.remove(helper.mesh);
    helper.mesh.geometry.dispose();
    const material: THREE.Material | THREE.Material[] = helper.mesh.material;
    if (Array.isArray(material)) {
      for (const mat of material) {
        mat.dispose();
      }
    } else {
      material.dispose();
    }
  }

  private dispose(): void {
    window.removeEventListener('pointermove', this.onPointerMove);
    this.canvas?.nativeElement?.removeEventListener('pointerdown', this.handleCanvasPointerDown);

    this.controls?.removeEventListener('change', this.startLooping);
    this.controls?.dispose();
    this.transformControls?.removeEventListener('dragging-changed', this.onTransformDraggingChanged);
    this.transformControls?.removeEventListener('objectChange', this.onTransformObjectChange);
    this.transformControls?.detach();
    this.transformControls?.dispose();
    this.dragControls?.dispose();

    const sceneObjects = this.sceneObjects;
    for (const object of sceneObjects) {
      this.scene.remove(object.lod);

      object.lod.traverse((object) => {
        if (object instanceof THREE.Mesh) {
          object.geometry.dispose();

          if (Array.isArray(object.material)) {
            object.material.forEach((material) => material.dispose());
          } else {
            object.material.dispose();
          }
        }
      });
    }

    for (const helper of this.helperObjects) {
      this.disposeHelperMesh(helper);
    }

    this.scene.traverse((child) => {
      const mesh: THREE.Mesh = child as THREE.Mesh;
      if (mesh.isMesh) {
        mesh.geometry?.dispose();
      }
    });

    this.helperOverlayTexture.dispose();
    this.tubeGeometry?.dispose();
    for (const helperShaderMaterial of this.helperShaderMaterials) {
      helperShaderMaterial.dispose();
    }
    this.helperShaderMaterials.length = 0;
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
    this.requestNextFrame();
  };

  private requestNextFrame(): void {
    if (this.frameRequested) {
      return;
    }

    this.frameRequested = true;
    window.requestAnimationFrame(this.loop);
  }

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

  private getLodDistanceForResolution(resolution: ResolutionLevel): number {
    switch (resolution) {
      case RESOLUTION_LEVEL.low:
        return 40;
      case RESOLUTION_LEVEL.medium:
        return 25;
      case RESOLUTION_LEVEL.high:
        return 0;
      default:
        return 0;
    }
  }
}

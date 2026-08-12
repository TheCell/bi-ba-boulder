import {
  AfterViewInit,
  Component,
  DestroyRef,
  effect,
  ElementRef,
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
import { LineDto, SceneMarking } from '@api-net/model/models';
import { fragmentShader, maxBoxMarkings, maxSphereMarkings, uniforms } from '../common/outdoor-shader-code';
import { resolveHelperColor } from '../common/outdoor-bloc-markings-types';
import {
  BoxSceneMarking,
  CustomSceneMarking,
  HelperShaderSnapshot,
  MaterialShader,
  SphereSceneMarking
} from '../outdoor-interfaces/scene-marking';
import { SceneMarkingForm } from '../../core/enums/scene-marking-form.enum';
import { createHelperOverlayTexture } from '../common/outdoor-bloc-utils';
import { RawModelInput } from './model-input.interface';
import { RESOLUTION_LEVEL, ResolutionLevel } from '../../interfaces/resolution-level';
import { fitCameraToCenteredObject } from '../common/camera-utils';

export interface EnhancedLine extends LineDto {
  lineColor: THREE.Color;
}

@Component({
  selector: 'app-outdoor-renderer',
  imports: [KeyboardShortcutsModule],
  templateUrl: './outdoor-renderer.html',
  styleUrl: './outdoor-renderer.scss',
  host: {
    '(window:resize)': 'onResize()'
  }
})
export class OutdoorRenderer implements AfterViewInit {
  private el: ElementRef = inject(ElementRef);
  private destroyRef = inject(DestroyRef);
  private cameraControlsService = inject(CameraControlsService);

  @ViewChild('canvas') public canvas: ElementRef = null!;

  public rawModels = input<RawModelInput[]>([]);
  public lines = input<EnhancedLine[]>();
  public selectedLine = input<{ line: LineDto; setFocus: boolean } | undefined>();
  public selected = output<{ line: LineDto; setFocus: boolean } | undefined>();

  private proccessedRawModels = signal<RawModelInput[]>([]);
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

  private helperShaderMaterials: THREE.MeshPhysicalMaterial[] = [];
  private raycaster: THREE.Raycaster = null!;
  private LINE_LAYER = 2;
  private LINE_RAYCAST_LAYER = 4;
  private loopCountSincePointerDown = 0;
  private readonly helperOverlayTexture: THREE.DataTexture = createHelperOverlayTexture();

  // markings
  private lightmarkingIntensity = 0.5;
  private readonly helperLayer = 3;
  private readonly helperObjects: CustomSceneMarking[] = [];
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
  private raycastTubeParams = {
    radius: 0.25,
    extrusionSegments: 100,
    radiusSegments: 6
  };

  private tubeGeometries: THREE.TubeGeometry[] = [];
  private tubeMeshes: THREE.Mesh[] = [];
  private rayVisionTubeMeshes: THREE.Mesh[] = [];
  private raycastTubeMeshes: THREE.Mesh[] = [];

  private sceneObjects: { lod: THREE.LOD; blocId: string }[] = [];
  private initialized = false; // temporary 'fix' for a timing problem

  public onResize(): void {
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

  public constructor() {
    effect(() => {
      const rawModels = this.rawModels();
      const proccessedRawModels = this.proccessedRawModels();
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
        let middlePoint = new THREE.Vector3(0, 0, 0);
        const mainLod = this.sceneObjects[0]?.lod;
        if (mainLod !== undefined) {
          mainLod.getCurrentLevel();
          mainLod.traverse((object) => {
            if (object instanceof THREE.Mesh) {
              const mesh = object as THREE.Mesh;
              middlePoint = mesh.geometry.boundingSphere?.center ?? middlePoint;
            }
          });
        }

        this.cameraControlsService.focusOnObject(lineObject, middlePoint);
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

    canvas.addEventListener('pointerdown', () => {
      this.loopCountSincePointerDown = 0;
    });
    canvas.addEventListener('pointerup', (event: PointerEvent) => {
      if (this.loopCountSincePointerDown < 2) {
        this.onPointerClick(event);
      }
    });

    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(this.LINE_RAYCAST_LAYER);

    this.regenerateLines();

    this.loop();
  }

  private loop = () => {
    if (!this.renderer) {
      return;
    }

    // const test = this.sceneObjects[0];
    // if (test !== undefined) {
    //   console.log(test.lod.getCurrentLevel());
    // }

    this.loopCountSincePointerDown++;
    this.renderer.render(this.scene, this.camera);
    // window.requestAnimationFrame(this.loop); // removed to not rerender on idle
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
          const lineWithInfos = this.selectedLine();

          if (lineWithInfos !== undefined && lineWithInfos.setFocus) {
            const lineObject = this.tubeMeshes.find((tubeMesh) => tubeMesh.userData['id'] === lineWithInfos.line.id);
            if (lineObject === undefined) {
              return;
            }

            let middlePoint = new THREE.Vector3(0, 0, 0);
            const mainLod = this.sceneObjects[0]?.lod;
            if (mainLod !== undefined) {
              mainLod.getCurrentLevel();
              mainLod.traverse((object) => {
                if (object instanceof THREE.Mesh) {
                  const mesh = object as THREE.Mesh;
                  middlePoint = mesh.geometry.boundingSphere?.center ?? middlePoint;
                }
              });
            }

            this.cameraControlsService.focusOnObject(lineObject, middlePoint);
          }
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
  //       this.scene.add(gltf.scene);
  //       // let childCounter = 0;
  //       gltf.scene.traverse((child) => {
  //         child.layers.set(1);
  //         // childCounter++;
  //         const mesh = child as THREE.Mesh;
  //         if (mesh.isMesh) {
  //           this.currentMesh = mesh;
  //           // this.originalBlockMaterial = mesh.material as THREE.MeshPhysicalMaterial;
  //           // this.originalBlockTexture = this.originalBlockMaterial.map;
  //           // this.originalBlockMaterial.needsUpdate = true;

  //           mesh.material = this.createHelperShaderMaterial(mesh.material);
  //         }
  //       });

  //       if (this.currentGltf !== undefined) {
  //         this.removeBoulderFromScene(this.currentGltf);
  //         this.currentGltf = gltf;
  //       } else {
  //         this.currentGltf = gltf;
  //         this.resetCameraPosition();
  //       }
  //       this.loop();

  //       // Re-apply uniforms after first render when shaders have compiled
  //       window.requestAnimationFrame(() => {
  //         this.applyShaderUniformUpdates();
  //         this.loop();
  //       });
  //     },
  //     (err: ErrorEvent) => {
  //       throw new Error(err.message);
  //     }
  //   );
  // }

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

    for (const raycastTubeMesh of this.raycastTubeMeshes) {
      this.scene.remove(raycastTubeMesh);
    }
    this.raycastTubeMeshes = [];

    const lines = this.lines();
    if (lines === undefined) {
      return;
    }
    const selectedLine = this.selectedLine();
    this.resetMarkings();

    if (selectedLine !== undefined) {
      const enhancedLine = this.lines()?.find((line) => line.id === selectedLine.line.id);
      if (enhancedLine !== undefined) {
        this.addLineToScene(enhancedLine, true);
      }

      for (const marking of selectedLine.line.data?.sceneMarkings ?? []) {
        // todo check sizes and rotation
        if (marking.form === SceneMarkingForm.Box) {
          this.addBoxMarking(marking);
        } else if (marking.form === SceneMarkingForm.Sphere) {
          this.addSphereMarking(marking);
        }

        // this.startLooping();
      }
    } else {
      for (const line of lines) {
        this.addLineToScene(line, false);
      }
    }

    this.updateMarkingShaderUniforms();
    this.applyShaderUniformUpdates();
    this.loop();
  }

  private addLineToScene(line: EnhancedLine, isHighlighted: boolean): void {
    if (line.data?.positions === undefined || line.data.positions.length < 3) {
      return;
    }

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
    const raycastMaterial = new THREE.MeshBasicMaterial({
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

    const raycastGeometry = new THREE.TubeGeometry(
      path,
      this.raycastTubeParams.extrusionSegments,
      this.raycastTubeParams.radius,
      this.raycastTubeParams.radiusSegments,
      false
    );
    const raycastTubeMesh = new THREE.Mesh(raycastGeometry, raycastMaterial);
    raycastTubeMesh.layers.set(this.LINE_RAYCAST_LAYER);
    raycastTubeMesh.userData = { id: line.id, identifier: line.identifier };
    this.raycastTubeMeshes.push(raycastTubeMesh);
    this.scene.add(raycastTubeMesh);
  }

  private addSphereMarking(sceneMarking: SceneMarking): void {
    if (
      sceneMarking.type === undefined ||
      sceneMarking.scale === undefined ||
      sceneMarking.quaternion === undefined ||
      sceneMarking.position === undefined
    ) {
      throw new Error('Scene marking is missing required properties: type, scale, quaternion, or position');
    }

    const color: THREE.Color = resolveHelperColor(sceneMarking.type);
    const mesh: THREE.Mesh = new THREE.Mesh(
      new THREE.SphereGeometry(1, 24, 24),
      new THREE.MeshStandardMaterial({
        color,
        transparent: true,
        opacity: 0.75,
        depthWrite: false
      })
    );
    const orientation: THREE.Quaternion = new THREE.Quaternion(
      sceneMarking.quaternion[0],
      sceneMarking.quaternion[1],
      sceneMarking.quaternion[2],
      sceneMarking.quaternion[3]
    );

    const position = new THREE.Vector3(sceneMarking.position[0], sceneMarking.position[1], sceneMarking.position[2]);

    mesh.position.copy(position);
    mesh.quaternion.copy(orientation);
    mesh.scale.setScalar(sceneMarking.scale[0]);
    mesh.layers.set(this.helperLayer);

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
    // this.selectHelper(helper);
    // this.updateMarkingShaderUniforms();
    // this.applyShaderUniformUpdates();
    // this.startLooping();
  }

  private addBoxMarking(sceneMarking: SceneMarking): void {
    if (
      sceneMarking.type === undefined ||
      sceneMarking.scale === undefined ||
      sceneMarking.quaternion === undefined ||
      sceneMarking.position === undefined
    ) {
      throw new Error('Scene marking is missing required properties: type, scale, quaternion, or position');
    }

    const color: THREE.Color = resolveHelperColor(sceneMarking.type);
    const mesh: THREE.Mesh = new THREE.Mesh(
      new THREE.BoxGeometry(1, 1, 1),
      new THREE.MeshStandardMaterial({
        color,
        transparent: true,
        opacity: 0.45,
        depthWrite: false
      })
    );

    // const helperNormal: THREE.Vector3 = this.intersection.normal.clone().normalize();
    const orientation: THREE.Quaternion = new THREE.Quaternion(
      sceneMarking.quaternion[0],
      sceneMarking.quaternion[1],
      sceneMarking.quaternion[2],
      sceneMarking.quaternion[3]
    );

    const position = new THREE.Vector3(sceneMarking.position[0], sceneMarking.position[1], sceneMarking.position[2]);
    mesh.position.copy(position);
    mesh.quaternion.copy(orientation);
    mesh.scale.set(sceneMarking.scale[0], sceneMarking.scale[1], sceneMarking.scale[2]);
    mesh.layers.set(this.helperLayer);

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
  }

  private resetMarkings(): void {
    for (const helper of this.helperObjects) {
      this.scene.remove(helper.mesh);
    }
    this.helperObjects.length = 0;
  }

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
    this.raycaster.intersectObjects(this.raycastTubeMeshes, false, currentIntersections);

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
    // material.wireframe = true;
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
      shader.uniforms['helperBlendStrength'] = { value: 0.0 };
      shader.uniforms['helperEmissiveStrength'] = { value: this.lightmarkingIntensity };
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

  private dispose(): void {
    this.controls?.removeEventListener('change', this.loop);
    this.controls?.dispose();
    this.canvas?.nativeElement?.removeEventListener('pointerdown', this.onPointerClick);

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

    this.scene.traverse((child) => {
      const mesh = child as THREE.Mesh;
      if (mesh.isMesh) {
        mesh.geometry?.dispose();
      }
    });

    this.renderer?.dispose();
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

import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, HostListener, OnDestroy, ViewChild } from '@angular/core';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import * as THREE from 'three';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { Line2 } from 'three/addons/lines/Line2.js';
import { LineGeometry } from 'three/addons/lines/LineGeometry.js';
import { LineMaterial } from 'three/addons/lines/LineMaterial.js';
import { MeshLine, MeshLineGeometry, MeshLineMaterial } from '@lume/three-meshline';
import ThreeMeshUI from 'three-mesh-ui';
import { HSLToHex } from '../utils/color-util';
import { ShortcutEventOutput, ShortcutInput } from 'ng-keyboard-shortcuts';
import { KeyboardShortcutsModule } from 'ng-keyboard-shortcuts';
import { BoulderProblemsService } from '../background-loading/boulder-problems.service';
import { BoulderLine } from '../api/interfaces/boulder-line';
import { Subject, Subscription } from 'rxjs';
import { ResolutionLevel } from '../api/interfaces/resolution-level';

@Component({
  selector: 'app-daone-render-test',
  standalone: true,
  imports: [
    CommonModule,
    KeyboardShortcutsModule
  ],
  templateUrl: './daone-render-test.component.html',
  styleUrl: './daone-render-test.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DaoneRenderTestComponent implements AfterViewInit, OnDestroy {
  @ViewChild('canvas') public canvas: ElementRef = null!;
  @HostListener('window:resize', ['$event']) public onResize(): void {
    if (this.renderer) {

      const canvasSizes = {
        width: this.el.nativeElement.offsetWidth,
        height: this.el.nativeElement.offsetHeight,
      };

      this.renderer.setPixelRatio(window.devicePixelRatio);
      this.renderer.setSize(canvasSizes.width, canvasSizes.height);
      this.camera.aspect = canvasSizes.width / canvasSizes.height;
      this.camera.updateProjectionMatrix();
    }
  }

  public shortcuts: ShortcutInput[] = [];

  private scene = new THREE.Scene();
  private loader = new GLTFLoader();
  private camera: THREE.PerspectiveCamera = null!;
  private controls: OrbitControls = null!;
  private renderer: THREE.WebGLRenderer = null!;
  private raycaster: THREE.Raycaster = null!;
  private meshLinePointer: MeshLine = null!;
  private meshLineGeometry = new MeshLineGeometry();
  private clickPoints: Array<THREE.Vector3> = [];
  private textBlocks: Array<ThreeMeshUI.Block> = [];
  private lineMaterials: Array<MeshLineMaterial> = [];
  private currentRandomRadius = Math.random() * 360;

  private finishedLoading = new Subject<ResolutionLevel>();
  private currentGltf?: GLTF;
  private subcription = new Subscription();

  public constructor(
    private boulderLoaderService: BoulderLoaderService,
    private boulderProblemsService: BoulderProblemsService,
    private el: ElementRef) {}

  public ngOnDestroy(): void {
    this.subcription.unsubscribe();
  }

  public ngAfterViewInit(): void {
    this.createCanvas();

    this.lineMaterials.push(this.getNewMeshLineMaterial());
    this.lineMaterials.push(this.getNewMeshLineMaterial());

    window.addEventListener( 'click', this.getClickCoordinate.bind(this) );

    this.shortcuts.push({
      key: ['ctrl + z'],
      preventDefault: true,
      command: (e: ShortcutEventOutput) => this.removeLastPoint()
    }, {
      key: ['ctrl + space'],
      preventDefault: true,
      command: (e: ShortcutEventOutput) => this.startNewLine()
    }, {
      key: ['ctrl + y'],
      preventDefault: true,
      command: (e: ShortcutEventOutput) => this.printClickPoints()
    });

    const testBoulder = this.boulderLoaderService.loadTestDaoneBoulder('low');
    testBoulder.subscribe({
      next: (data) => {
        this.removePreviousAndAddBoulderToScene(data);
        this.finishedLoading.next('low');
      }
    });

    this.subcription.add(this.finishedLoading.subscribe({
      next: (resolutionLevel: ResolutionLevel) => {
        if (resolutionLevel === 'low') {
          this.boulderLoaderService.loadTestDaoneBoulder('medium').subscribe({
            next: (data) => {
              this.removePreviousAndAddBoulderToScene(data);
              this.finishedLoading.next('medium');
            }
          });
        } else if (resolutionLevel === 'medium') {
          this.boulderLoaderService.loadTestDaoneBoulder('high').subscribe({
            next: (data) => {
              this.removePreviousAndAddBoulderToScene(data);
              this.finishedLoading.next('high');
            }
          });
        }
      }
    }));


    const testRoutes = this.boulderProblemsService.loadDaoneTestBoulderProblem()
    testRoutes.subscribe({
      next: (data: Array<BoulderLine>) => {
        data.forEach((boulderLine: BoulderLine) => {
          this.addLineToScene(this.scene, boulderLine.points.map((point) => new THREE.Vector3(point.x, point.y, point.z)), boulderLine.color)
        });
      }
    });
  }

  private createCanvas(): void {
    const canvas = this.canvas.nativeElement;
    if (!canvas) {
      return;
    }

    const canvasSizes = {
      width: canvas.offsetWidth,
      height: canvas.offsetHeight,
    };

    this.renderer = new THREE.WebGLRenderer({
      logarithmicDepthBuffer: true,
      canvas: canvas,
      alpha: true
    });
    this.renderer.setClearColor( 0x000000, 0 );

    this.camera = new THREE.PerspectiveCamera(
      75,
      canvasSizes.width / canvasSizes.height,
      0.001,
      1000
    );
    this.camera.layers.enable(0);
    this.camera.layers.enable(1);

    this.onResize();
    this.scene.add(this.camera);

    this.addLights(this.scene);
    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    // controls.keys = {
    //   LEFT: 'ArrowLeft', //left arrow
    //   UP: 'ArrowUp', // up arrow
    //   RIGHT: 'ArrowRight', // right arrow
    //   BOTTOM: 'ArrowDown' // down arrow
    // }

    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(1); // only hit layer 1 objects

    this.loop();
  }

  private loop = () => {
    ThreeMeshUI.update();
    this.updateTextRotation();
    this.renderer.render(this.scene, this.camera);
    window.requestAnimationFrame(this.loop);
  }

  private updateTextRotation(): void {
    this.textBlocks.forEach((block) => {
      block.lookAt(this.camera.position);
    });
  }

  private removePreviousAndAddBoulderToScene(buffer: ArrayBuffer): void {
    this.loader.parse(buffer, '', (gltf: GLTF) => {

      this.scene.add(gltf.scene);
      gltf.scene.traverse((child) => {
        child.layers.set(1); // set hit layer
      });

      if (this.currentGltf !== undefined) {
        this.removeBoulderFromScene(this.currentGltf);
      } else {
        this.fitCameraToCenteredObject(this.camera, gltf.scene, 0, this.controls);
      }
      // this.drawBoundingBox(gltf.scene);
      // this.addRoutes(this.scene);
      this.currentGltf = gltf;
    },
    (err: ErrorEvent) => {
      throw new Error(err.message);
    });
  }

  private removeBoulderFromScene(gltf: GLTF): void {
    this.scene.remove(gltf.scene);
  }

  private drawBoundingBox(scene: THREE.Group<THREE.Object3DEventMap>): void {
    var bbox = new THREE.Box3().setFromObject(scene);
    const helper = new THREE.Box3Helper( bbox, 0xffff00 );
    this.scene.add( helper );
  }

  // source: https://wejn.org/2020/12/cracking-the-threejs-object-fitting-nut/
  private fitCameraToCenteredObject(camera: THREE.PerspectiveCamera, object: THREE.Group<THREE.Object3DEventMap>, offset?: number, orbitControls?: OrbitControls): void {
    const boundingBox = new THREE.Box3();
    boundingBox.setFromObject( object );

    // var middle = new THREE.Vector3();
    var size = new THREE.Vector3();
    boundingBox.getSize(size);

    // figure out how to fit the box in the view:
    // 1. figure out horizontal FOV (on non-1.0 aspects)
    // 2. figure out distance from the object in X and Y planes
    // 3. select the max distance (to fit both sides in)
    //
    // The reason is as follows:
    //
    // Imagine a bounding box (BB) is centered at (0,0,0).
    // Camera has vertical FOV (camera.fov) and horizontal FOV
    // (camera.fov scaled by aspect, see fovh below)
    //
    // Therefore if you want to put the entire object into the field of view,
    // you have to compute the distance as: z/2 (half of Z size of the BB
    // protruding towards us) plus for both X and Y size of BB you have to
    // figure out the distance created by the appropriate FOV.
    //
    // The FOV is always a triangle:
    //
    //  (size/2)
    // +--------+
    // |       /
    // |      /
    // |     /
    // | F° /
    // |   /
    // |  /
    // | /
    // |/
    //
    // F° is half of respective FOV, so to compute the distance (the length
    // of the straight line) one has to: `size/2 / Math.tan(F)`.
    //
    // FTR, from https://threejs.org/docs/#api/en/cameras/PerspectiveCamera
    // the camera.fov is the vertical FOV.

    const fov = camera.fov * ( Math.PI / 180 );
    const fovh = 2*Math.atan(Math.tan(fov/2) * camera.aspect);
    let dx = size.z / 2 + Math.abs( size.x / 2 / Math.tan( fovh / 2 ) );
    let dy = size.z / 2 + Math.abs( size.y / 2 / Math.tan( fov / 2 ) );
    let cameraZ = Math.max(dx, dy);

    // offset the camera, if desired (to avoid filling the whole canvas)
    if( offset !== undefined && offset !== 0 ) {
      cameraZ *= offset;
    }

    camera.position.set( 0, 0, cameraZ );

    // set the far plane of the camera so that it easily encompasses the whole object
    const minZ = boundingBox.min.z;
    const cameraToFarEdge = ( minZ < 0 ) ? -minZ + cameraZ : cameraZ - minZ;

    camera.far = cameraToFarEdge * 3;
    camera.updateProjectionMatrix();

    if ( orbitControls !== undefined ) {
        // set camera to rotate around the center
        orbitControls.target = new THREE.Vector3(0, 0, 0);

        // prevent camera from zooming out far enough to create far plane cutoff
        orbitControls.maxDistance = cameraToFarEdge * 2;
    }
  };

  private addLights(scene: THREE.Scene): void {
    const ambientLight = new THREE.AmbientLight(0xffffff, 10.5);
    scene.add(ambientLight);

    // const pointLight = new THREE.PointLight(0xffffff, 0.5);
    // pointLight.position.x = 2;
    // pointLight.position.y = 2;
    // pointLight.position.z = 2;
    // scene.add(pointLight);
  }

  private addRoutes(scene: THREE.Scene): void {
    const points = [];
    for (let j = 0; j < Math.PI; j += (2 * Math.PI) / 100) {
      points.push(Math.cos(j) * 2.5, Math.sin(j) * 2.5, 0);
    }

    // const line = new MeshLine();
    const geometry = new MeshLineGeometry();
    geometry.setPoints(points);
    geometry.setPoints(points, p => 2 + Math.sin(50 * p)); // makes width sinusoidal
    const line = new MeshLine(geometry, this.lineMaterials[0]);
    scene.add(line);
  }

  private getClickCoordinate(event: Event): void {
    if (this.scene == undefined || this.scene.children == undefined) {
      return;
    }

    const mouseEvent = event as MouseEvent;
    let pointer = new THREE.Vector2();

    pointer.x = (mouseEvent.clientX / this.canvas.nativeElement.offsetWidth) * 2 - 1;
    pointer.y = - (mouseEvent.clientY / this.canvas.nativeElement.offsetHeight) * 2 + 1;

    this.raycaster.setFromCamera(pointer, this.camera);

    const intersects = this.raycaster.intersectObjects(this.scene.children);
    // this.scene.add(new THREE.ArrowHelper(this.raycaster.ray.direction, this.raycaster.ray.origin, 300, 0xff0000));

    if (intersects.length === 0) {
      return;
    }

    const normal = intersects[0].normal ?? new THREE.Vector3(0, 0, 0);
    const newPoint: THREE.Vector3 = new THREE.Vector3 (
      intersects[0].point.x + 0.2 * normal.x,
      intersects[0].point.y + 0.2 * normal.y,
      intersects[0].point.z + 0.2 * normal.z
    );
    this.clickPoints.push(newPoint);
    if (this.clickPoints.length < 2) {
      return;
    }

    this.drawLineFromActivePoints(this.scene);
    // todo morphAttributes für hover benutzen? (https://github.com/mrdoob/three.js/blob/master/examples/webgl_buffergeometry_lines.html)
  }

  private drawLineFromActivePoints(scene: THREE.Scene): void {
    if (this.clickPoints.length === 2) {
      const middlePoint = new THREE.Vector3();
      middlePoint.lerpVectors(this.clickPoints[0], this.clickPoints[1], 0.5);
      this.addText(this.scene, 'A2', middlePoint);
    }

    this.meshLineGeometry = new MeshLineGeometry();
    this.meshLineGeometry.setPoints(this.clickPoints);

    scene.remove(this.meshLinePointer);
    this.meshLinePointer = new MeshLine(this.meshLineGeometry, this.lineMaterials[this.lineMaterials.length - 1]);
    scene.add(this.meshLinePointer);
  }

  private addLineToScene(scene: THREE.Scene, points: Array<THREE.Vector3>, color: string): void {
    // const geometry = new MeshLineGeometry();
    // geometry.setPoints(points);
    // const meshLineMaterial = this.getNewMeshLineMaterial(color);
    // const line = new MeshLine(geometry, meshLineMaterial);
    // scene.add(line);

    const material = this.getNewLineBasicMaterial(color);
    // const geometry2 = new THREE.BufferGeometry().setFromPoints(points);
    const geometry2 = new LineGeometry();
    geometry2.setPositions(points.flatMap((value) => [value.x, value.y, value.z]));
    const line2 = new Line2(geometry2, material);
    scene.add(line2);
  }

  private removeLastPoint(): void {
    this.clickPoints.pop();
    this.drawLineFromActivePoints(this.scene);
  }

  private addText(scene: THREE.Scene, text: string, position: THREE.Vector3): void {
    const container = new ThreeMeshUI.Block({
      width: 0.3,
      height: 0.3,
      padding: 0.1,
      fontFamily: './font/Roboto-msdf.json',
      fontTexture: './font/Roboto-msdf.png',
      justifyContent: 'center',
     });
     container.position.set(position.x, position.y, position.z);
     container.lookAt(this.camera.position);
     container.position.lerpVectors(container.position, this.camera.position, 0.05);

     const textMesh = new ThreeMeshUI.Text({
      content: text
     });

     container.add(textMesh);
     this.textBlocks.push(container);
     scene.add(container);
  }

  private getNewMeshLineMaterial(color?: string): MeshLineMaterial {
    return new MeshLineMaterial({
      useMap: false,
      color: new THREE.Color(color ?? this.getRandomColor()),
      opacity: 1,
      resolution: new THREE.Vector2(this.canvas.nativeElement.offsetWidth, this.canvas.nativeElement.offsetHeight),
      sizeAttenuation: false,
      lineWidth: 10
    } as any);
  }

  private getNewLineBasicMaterial(color?: string): LineMaterial {
    return new LineMaterial({
      color: new THREE.Color(color ?? this.getRandomColor()),
      opacity: 1,
      linewidth: 5,
      resolution: new THREE.Vector2(this.canvas.nativeElement.offsetWidth, this.canvas.nativeElement.offsetHeight),
      dashed: false,
    });
  }

  private printClickPoints(): void {
    console.log('Click Points:', this.clickPoints);
  }

  private startNewLine(): void {
    this.lineMaterials.push(this.getNewMeshLineMaterial());
    this.clickPoints = [];
    this.meshLinePointer = new MeshLine(this.meshLineGeometry, this.lineMaterials[this.lineMaterials.length - 1]);
  }

  private getRandomColor(): string {
    const currentRadius =  this.currentRandomRadius;
    const randomColor = HSLToHex({ h: currentRadius, s: 70, l: 80});
    this.currentRandomRadius *= Math.E;
    this.currentRandomRadius %= 360;
    return randomColor;
  }
}

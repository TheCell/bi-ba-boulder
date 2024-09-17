import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, HostListener, ViewChild } from '@angular/core';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import * as THREE from 'three';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import {MeshLine, MeshLineGeometry, MeshLineMaterial} from '@lume/three-meshline';

@Component({
  selector: 'app-boulder-render',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './boulder-render.component.html',
  styleUrl: './boulder-render.component.scss',
})
export class BoulderRenderComponent implements AfterViewInit {
  @ViewChild('canvas') public canvas: ElementRef = null!;
  @HostListener('window:resize', ['$event']) public onResize(): void {
    if (this.renderer) {

      const canvasSizes = {
        width: this.el.nativeElement.offsetWidth,
        height: this.el.nativeElement.offsetHeight,
      };

      this.renderer.setSize(canvasSizes.width, canvasSizes.height);
      this.camera.aspect = canvasSizes.width / canvasSizes.height;
      this.camera.updateProjectionMatrix();
    }
  }

  private scene = new THREE.Scene();
  private loader = new GLTFLoader();
  private camera: THREE.PerspectiveCamera = null!;
  private controls: OrbitControls = null!;
  private renderer: THREE.WebGLRenderer = null!;

  public constructor(
    private boulderLoaderService: BoulderLoaderService,
    private el: ElementRef) {}

  public ngAfterViewInit(): void {
    this.createCanvas();
    const testBoulder = this.boulderLoaderService.loadTestBoulder();
    testBoulder.subscribe({
      next: (data) => {
        this.addBoulderToScene(data);
      }
    });
  }

  private createCanvas(): void {
    const canvasSizes = {
      width: this.el.nativeElement.offsetWidth,
      height: this.el.nativeElement.offsetHeight,
    };

    const canvas = this.canvas.nativeElement;
    if (!canvas) {
      return;
    }

    this.renderer = new THREE.WebGLRenderer({
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

    const animateGeometry = () => {
      this.renderer.render(this.scene, this.camera);
      window.requestAnimationFrame(animateGeometry);
    };

    animateGeometry();
  }

  private addBoulderToScene(buffer: ArrayBuffer): void {
    this.loader.parse(buffer, '', (gltf: GLTF) => {
      console.log(gltf);

      this.scene.add(gltf.scene);
      // this.drawBoundingBox(gltf.scene);
      this.fitCameraToCenteredObject(this.camera, gltf.scene, 0, this.controls);
      this.addRoutes(this.scene);
    },
    (err: ErrorEvent) => {
      throw new Error(err.message);
    });
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
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.5);
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
      points.push(Math.cos(j) * 10, Math.sin(j) * 10, 0);
    }

    // const line = new MeshLine();
    const geometry = new MeshLineGeometry();
    geometry.setPoints(points);
    geometry.setPoints(points, p => 2 + Math.sin(50 * p)); // makes width sinusoidal
  //   const material = new MeshLineMaterial( {
  //     color: 0xffffff,
  //     linewidth: 1,
  //     scale: 1,
  //     dashSize: 3,
  //     gapSize: 1,
  // } );
    // const material = new THREE.MeshBasicMaterial();
    const material = new MeshLineMaterial({
      useMap: false,
      color: new THREE.Color(0xffaadd),
      opacity: 1,
      resolution: new THREE.Vector2(this.el.nativeElement.offsetWidth, this.el.nativeElement.offsetHeight),
      sizeAttenuation: false,
      lineWidth: 10,
    } as any)
    const line = new MeshLine(geometry, material);
    scene.add(line);

    const raycaster = new THREE.Raycaster();
    // Use raycaster as usual:
    raycaster.intersectObject(line);
  }
}

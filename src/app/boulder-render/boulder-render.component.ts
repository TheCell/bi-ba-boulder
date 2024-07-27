import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import * as THREE from 'three';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

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
  @ViewChild('canvas') canvas: any;

  private scene = new THREE.Scene();
  private loader = new GLTFLoader();

  public constructor(private boulderLoaderService: BoulderLoaderService) {}

  public ngAfterViewInit(): void {
    this.createCanvas();
    const testBoulder = this.boulderLoaderService.loadTestBoulder();
    testBoulder.subscribe({
      next: (data) => {
        this.addBoulderToScene(data);
      }
    })
  }

  private createCanvas(): void {
    const canvas = this.canvas.nativeElement;

    const material = new THREE.MeshToonMaterial();

    const ambientLight = new THREE.AmbientLight(0xffffff, 0.5);
    this.scene.add(ambientLight);

    const pointLight = new THREE.PointLight(0xffffff, 0.5);
    pointLight.position.x = 2;
    pointLight.position.y = 2;
    pointLight.position.z = 2;
    this.scene.add(pointLight);

    const box = new THREE.Mesh(
      new THREE.BoxGeometry(1.5, 1.5, 1.5),
      material
    );

    const torus = new THREE.Mesh(
      new THREE.TorusGeometry(5, 1.5, 16, 100),
      material
    );

    this.scene.add(torus, box);

    const canvasSizes = {
      width: window.innerWidth,
      height: window.innerHeight,
    };

    const camera = new THREE.PerspectiveCamera(
      75,
      canvasSizes.width / canvasSizes.height,
      0.001,
      1000
    );
    camera.position.z = 30;
    this.scene.add(camera);

    if (!canvas) {
      return;
    }

    const renderer = new THREE.WebGLRenderer({
      canvas: canvas,
    });
    renderer.setClearColor(0xe232222, 1);
    renderer.setSize(canvasSizes.width, canvasSizes.height);

    //  window.addEventListener('resize', () => {
    //   canvasSizes.width = window.innerWidth;
    //   canvasSizes.height = window.innerHeight;

    //   camera.aspect = canvasSizes.width / canvasSizes.height;
    //   camera.updateProjectionMatrix();

    //   renderer.setSize(canvasSizes.width, canvasSizes.height);
    //   renderer.render(scene, camera);
    // });

    const clock = new THREE.Clock();
    const controls = new OrbitControls(camera, renderer.domElement);
    // controls.keys = {
    //   LEFT: 'ArrowLeft', //left arrow
    //   UP: 'ArrowUp', // up arrow
    //   RIGHT: 'ArrowRight', // right arrow
    //   BOTTOM: 'ArrowDown' // down arrow
    // }

    const animateGeometry = () => {
      const elapsedTime = clock.getElapsedTime();

      // Update animation objects
      box.rotation.x = elapsedTime;
      box.rotation.y = elapsedTime;
      box.rotation.z = elapsedTime;

      torus.rotation.x = -elapsedTime;
      torus.rotation.y = -elapsedTime;
      torus.rotation.z = -elapsedTime;

      controls.update();
      // console.log(controls.object.position);


      // Render
      renderer.render(this.scene, camera);

      // Call animateGeometry again on the next frame
      window.requestAnimationFrame(animateGeometry);
    };

    animateGeometry();
  }

  private addBoulderToScene(buffer: ArrayBuffer): void {
    const boulder = this.loader.parse(buffer, '', (gltf: GLTF) => {
      console.log(gltf);

      gltf.scene.position.x = 15;
      gltf.scene.position.y = 0;
      gltf.scene.position.z = 20;
      this.scene.add(gltf.scene);
    },
    (err: ErrorEvent) => {
      throw new Error(err.message);
    });
  }
}

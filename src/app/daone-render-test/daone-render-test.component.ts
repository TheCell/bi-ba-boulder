import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, computed, effect, ElementRef, HostListener, input, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { Line2 } from 'three/addons/lines/Line2.js';
import { LineGeometry } from 'three/addons/lines/LineGeometry.js';
import { LineMaterial } from 'three/addons/lines/LineMaterial.js';
import { HSLToHex } from '../utils/color-util';
import { ShortcutInput } from 'ng-keyboard-shortcuts';
import { KeyboardShortcutsModule } from 'ng-keyboard-shortcuts';
import { Subscription } from 'rxjs';
import { fitCameraToCenteredObject } from '../utils/camera-utils';

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
export class DaoneRenderTestComponent implements AfterViewInit {
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

  public rawModel = input<ArrayBuffer>();
  public testInput = input<boolean>(false);

  public shortcuts: ShortcutInput[] = [];

  private proccessedRawModel?: ArrayBuffer;
  private scene = new THREE.Scene();
  private loader = new GLTFLoader();
  private camera: THREE.PerspectiveCamera = null!;
  private controls: OrbitControls = null!;
  private renderer: THREE.WebGLRenderer = null!;
  private raycaster: THREE.Raycaster = null!;
  private currentRandomRadius = Math.random() * 360;

  private currentGltf?: GLTF;

  public constructor(private el: ElementRef) {
    effect(() => {
      const rawModel = this.rawModel();
      if (rawModel !== this.proccessedRawModel) {
        this.proccessedRawModel = rawModel;
        if (rawModel !== undefined) {
          this.removePreviousAndAddBoulderToScene(rawModel);
        }
        console.log('Model changed');
      }
    });
  }

  public ngAfterViewInit(): void {
    this.createCanvas();
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

    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(1);

    this.loop();
  }

  private loop = () => {
    this.renderer.render(this.scene, this.camera);
    window.requestAnimationFrame(this.loop);
  }

  private removePreviousAndAddBoulderToScene(buffer: ArrayBuffer): void {
    this.loader.parse(buffer, '', (gltf: GLTF) => {
      this.scene.add(gltf.scene);
      gltf.scene.traverse((child) => {
        child.layers.set(1);
      });

      if (this.currentGltf !== undefined) {
        this.removeBoulderFromScene(this.currentGltf);
      } else {
        fitCameraToCenteredObject(this.camera, gltf.scene, 0, this.controls);
      }
      this.currentGltf = gltf;
    },
    (err: ErrorEvent) => {
      throw new Error(err.message);
    });
  }

  private removeBoulderFromScene(gltf: GLTF): void {
    this.scene.remove(gltf.scene);
  }


  private addLights(scene: THREE.Scene): void {
    const ambientLight = new THREE.AmbientLight(0xffffff, 10.5);
    scene.add(ambientLight);
  }

  private addLineToScene(scene: THREE.Scene, points: Array<THREE.Vector3>, color: string): void {
    const material = this.getNewLineMaterial(color);
    const geometry = new LineGeometry();
    geometry.setPositions(points.flatMap((value) => [value.x, value.y, value.z]));
    const line2 = new Line2(geometry, material);
    scene.add(line2);
  }

  private getNewLineMaterial(color?: string): LineMaterial {
    return new LineMaterial({
      color: new THREE.Color(color ?? this.getRandomColor()),
      opacity: 1,
      linewidth: 5,
      resolution: new THREE.Vector2(this.canvas.nativeElement.offsetWidth, this.canvas.nativeElement.offsetHeight),
      dashed: false,
    });
  }

  private getRandomColor(): string {
    const currentRadius =  this.currentRandomRadius;
    const randomColor = HSLToHex({ h: currentRadius, s: 70, l: 80});
    this.currentRandomRadius *= Math.E;
    this.currentRandomRadius %= 360;
    return randomColor;
  }
}

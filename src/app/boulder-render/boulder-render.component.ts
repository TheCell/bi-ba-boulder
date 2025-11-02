import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, effect, ElementRef, HostListener, input, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { KeyboardShortcutsModule, ShortcutInput } from 'ng-keyboard-shortcuts';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { Line2 } from 'three/examples/jsm/lines/Line2.js';
import { LineMaterial } from 'three/addons/lines/LineMaterial.js';
import { LineGeometry } from 'three/addons/lines/LineGeometry.js';
import { GLTFLoader, GLTF } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { BoulderLine } from '../interfaces/boulder-line';
import { fitCameraToCenteredObject } from '../utils/camera-utils';
import { HSLToHex } from '../utils/color-util';
import { EffectComposer } from 'three/addons/postprocessing/EffectComposer.js';
import { OutlinePass } from 'three/examples/jsm/postprocessing/OutlinePass.js';
import { RenderPass } from 'three/examples/jsm/postprocessing/RenderPass.js';
import { OutputPass } from 'three/examples/jsm/postprocessing/OutputPass.js';
import { ShaderPass } from 'three/examples/jsm/postprocessing/ShaderPass.js';
import { FXAAShader } from 'three/examples/jsm/shaders/FXAAShader.js';

@Component({
  selector: 'app-boulder-render',
  imports: [
    CommonModule,
    KeyboardShortcutsModule
  ],
  templateUrl: './boulder-render.component.html',
  styleUrl: './boulder-render.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BoulderRenderComponent implements AfterViewInit {
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
  public rawModel = input<ArrayBuffer>();
  public lines = input<BoulderLine[]>();

  private proccessedRawModel?: ArrayBuffer;
  private processedLines: BoulderLine[] = [];
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
      }
    });
  }

  public ngAfterViewInit(): void {
    this.createCanvas();

    const lines = this.lines();
    if (lines !== this.processedLines) {
      if (lines !== undefined) {
        lines.forEach((line: BoulderLine) => {
          this.addLineToScene(this.scene, line.points.map((point) => new THREE.Vector3(point.x, point.y, point.z)), line.color);
        });

        this.processedLines = lines;
      }
    }
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
    window.requestAnimationFrame(this.loop);
    this.renderer.render(this.scene, this.camera);
    // console.log(`Render time: ${endTime - timer} ms`, `Drawcalls: ${this.renderer.info.render.calls} calls`);
  }

  private removePreviousAndAddBoulderToScene(buffer: ArrayBuffer): void {
    this.loader.parse(buffer, '', (gltf: GLTF) => {
      // console.log('gltf', gltf);
      
      this.scene.add(gltf.scene);
      let childCounter = 0;
      gltf.scene.traverse((child) => {
        child.layers.set(1);
        childCounter++;
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
    const ambientLight = new THREE.AmbientLight(0xffffff, 3);
    scene.add(ambientLight);
  }

  private addLineToScene(scene: THREE.Scene, points: THREE.Vector3[], color?: string): void {
    const material = this.getNewLineMaterial(color);
    const geometry = new LineGeometry();
    geometry.setPositions(points.flatMap((value) => [value.x, value.y, value.z]));
    const line2 = new Line2(geometry, material);
    scene.add(line2);
    // todo morphAttributes für hover benutzen? (https://github.com/mrdoob/three.js/blob/master/examples/webgl_buffergeometry_lines.html)
  }

  private getNewLineMaterial(color?: string): LineMaterial {
    return new LineMaterial({
      color: new THREE.Color(color ?? this.getRandomColor()),
      opacity: 1,
      linewidth: 5,
      resolution: new THREE.Vector2(this.canvas.nativeElement.offsetWidth, this.canvas.nativeElement.offsetHeight), // TODO this will have to be recalculated on resize?
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


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
      if (this.startHoldOutlinePass) {
        this.startHoldOutlinePass.setSize(canvasSizes.width, canvasSizes.height);
        this.startHoldOutlinePass.resolution.set(canvasSizes.width, canvasSizes.height);
      }
      if (this.topHoldOutlinePass) {
        this.topHoldOutlinePass.setSize(canvasSizes.width, canvasSizes.height);
        this.topHoldOutlinePass.resolution.set(canvasSizes.width, canvasSizes.height);
      }
      if (this.footOnlyHoldOutlinePass) {
        this.footOnlyHoldOutlinePass.setSize(canvasSizes.width, canvasSizes.height);
        this.footOnlyHoldOutlinePass.resolution.set(canvasSizes.width, canvasSizes.height);
      }
      if (this.normalHoldOutlinePass) {
        this.normalHoldOutlinePass.setSize(canvasSizes.width, canvasSizes.height);
        this.normalHoldOutlinePass.resolution.set(canvasSizes.width, canvasSizes.height);
      }
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
  
  // outline objects
  private composer: EffectComposer = null!;
  private selectedStartHoldObjects: THREE.Object3D[] = [];
  private selectedTopHoldObjects: THREE.Object3D[] = [];
  private selectedFootHoldObjects: THREE.Object3D[] = [];
  private selectedNormalHoldObjects: THREE.Object3D[] = [];
  private startHoldOutlinePass: OutlinePass = null!;
  private topHoldOutlinePass: OutlinePass = null!;
  private footOnlyHoldOutlinePass: OutlinePass = null!;
  private normalHoldOutlinePass: OutlinePass = null!;

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

    this.setupComposer();
    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(1);


    this.loop();
  }

  private loop = () => {
    const timer = Date.now();
    // this.renderer.render(this.scene, this.camera);
    this.composer.render();
    const endTime = Date.now();
    
    window.requestAnimationFrame(this.loop);
    console.log(`Render time: ${endTime - timer} ms`);
  }

  private removePreviousAndAddBoulderToScene(buffer: ArrayBuffer): void {
    this.loader.parse(buffer, '', (gltf: GLTF) => {
      // console.log('gltf', gltf);
      
      this.scene.add(gltf.scene);
      let childCounter = 0;
      gltf.scene.traverse((child) => {
        if (childCounter > 1) {
          // todo: only highlight objects for active routes
          switch (childCounter % 4) {
            case 0:
              this.selectedStartHoldObjects.push(child);
              break;
            case 1:
              this.selectedFootHoldObjects.push(child);
              break;
            case 2:
              this.selectedNormalHoldObjects.push(child);
              break;
            case 3:
              this.selectedTopHoldObjects.push(child);
          }
        }
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

  private setupComposer(): void {
    this.composer = new EffectComposer(this.renderer);
    const renderPass = new RenderPass(this.scene, this.camera);
    this.composer.addPass(renderPass);

    // this.startHoldOutlinePass = new OutlinePass(new THREE.Vector2(this.canvas.nativeElement.offsetWidth, this.canvas.nativeElement.offsetHeight), this.scene, this.camera);
    // this.startHoldOutlinePass.visibleEdgeColor.set(new THREE.Color(0x6ce35b));
    // this.startHoldOutlinePass.hiddenEdgeColor.set(0, 0, 0);
    // this.startHoldOutlinePass.edgeStrength = 2;
    // this.startHoldOutlinePass.edgeGlow = 0;
    // this.startHoldOutlinePass.edgeThickness = 1;
    // this.startHoldOutlinePass.oldClearColor = new THREE.Color(0x000000);
    // this.composer.addPass(this.startHoldOutlinePass);
    // this.startHoldOutlinePass.selectedObjects = this.selectedStartHoldObjects;

    // this.topHoldOutlinePass = new OutlinePass(new THREE.Vector2(this.canvas.nativeElement.offsetWidth, this.canvas.nativeElement.offsetHeight), this.scene, this.camera);
    // // this.topHoldOutlinePass.visibleEdgeColor.set(236, 79, 240);
    // this.topHoldOutlinePass.visibleEdgeColor.set(new THREE.Color(0xec4ff0));
    // this.topHoldOutlinePass.hiddenEdgeColor.set(0, 0, 0);
    // this.topHoldOutlinePass.edgeStrength = 2;
    // this.topHoldOutlinePass.edgeGlow = 0;
    // this.topHoldOutlinePass.edgeThickness = 1;
    // this.topHoldOutlinePass.oldClearColor = new THREE.Color(0x000000);
    // this.composer.addPass(this.topHoldOutlinePass);
    // this.topHoldOutlinePass.selectedObjects = this.selectedTopHoldObjects;

    // this.footOnlyHoldOutlinePass = new OutlinePass(new THREE.Vector2(this.canvas.nativeElement.offsetWidth, this.canvas.nativeElement.offsetHeight), this.scene, this.camera);
    // // this.footOnlyHoldOutlinePass.visibleEdgeColor.set(238, 242, 77);
    // this.footOnlyHoldOutlinePass.visibleEdgeColor.set(new THREE.Color(0xeef24d));
    // this.footOnlyHoldOutlinePass.hiddenEdgeColor.set(0, 0, 0);
    // this.footOnlyHoldOutlinePass.edgeStrength = 2;
    // this.footOnlyHoldOutlinePass.edgeGlow = 0;
    // this.footOnlyHoldOutlinePass.edgeThickness = 1;
    // this.footOnlyHoldOutlinePass.oldClearColor = new THREE.Color(0x000000);
    // this.composer.addPass(this.footOnlyHoldOutlinePass);
    // this.footOnlyHoldOutlinePass.selectedObjects = this.selectedFootHoldObjects;

    // this.normalHoldOutlinePass = new OutlinePass(new THREE.Vector2(this.canvas.nativeElement.offsetWidth, this.canvas.nativeElement.offsetHeight), this.scene, this.camera);
    // // this.normalHoldOutlinePass.visibleEdgeColor.set(79, 204, 240);
    // this.normalHoldOutlinePass.visibleEdgeColor.set(new THREE.Color(0x4fccf0));
    // this.normalHoldOutlinePass.hiddenEdgeColor.set(0, 0, 0);
    // this.normalHoldOutlinePass.edgeStrength = 2;
    // this.normalHoldOutlinePass.edgeGlow = 0;
    // this.normalHoldOutlinePass.edgeThickness = 1;
    // this.normalHoldOutlinePass.oldClearColor = new THREE.Color(0x000000);
    // this.composer.addPass(this.normalHoldOutlinePass);
    // this.normalHoldOutlinePass.selectedObjects = this.selectedNormalHoldObjects;

    const outputPass = new OutputPass();
    this.composer.addPass(outputPass);

    const effectFXAA = new ShaderPass( FXAAShader );
    effectFXAA.uniforms[ 'resolution' ].value.set( 1 / this.canvas.nativeElement.offsetWidth, 1 / this.canvas.nativeElement.offsetHeight );
    this.composer.addPass( effectFXAA );
  }
}


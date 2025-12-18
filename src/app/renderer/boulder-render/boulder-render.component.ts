
import { AfterViewInit, ChangeDetectionStrategy, Component, effect, ElementRef, HostListener, inject, input, OnInit, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { KeyboardShortcutsModule, ShortcutInput } from 'ng-keyboard-shortcuts';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { Line2 } from 'three/examples/jsm/lines/Line2.js';
import { LineMaterial } from 'three/addons/lines/LineMaterial.js';
import { LineGeometry } from 'three/addons/lines/LineGeometry.js';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { BoulderLine } from '../../interfaces/boulder-line';
import { fitCameraToCenteredObject } from '../common/camera-utils';
import { HSLToHex } from '../../utils/color-util';
import { beginVertex, mapFragment, uniforms, vViewPositionReplace, worldposVertex } from '../common/shader-code';
import { ActivatedRoute } from '@angular/router';
import { SpraywallProblemDto } from '@api/index';

@Component({
  selector: 'app-boulder-render',
  imports: [
    KeyboardShortcutsModule
  ],
  templateUrl: './boulder-render.component.html',
  styleUrl: './boulder-render.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BoulderRenderComponent implements OnInit, AfterViewInit {
  private el: ElementRef = inject(ElementRef);

  @ViewChild('canvas') public canvas: ElementRef = null!;
  @HostListener('window:resize') public onResize(): void {
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
  public boulderProblem = input<SpraywallProblemDto>();

  private proccessedRawModel?: ArrayBuffer;
  private processedLines: BoulderLine[] = [];
  private scene = new THREE.Scene();
  private loader = new GLTFLoader();
  private camera: THREE.PerspectiveCamera = null!;
  private controls: OrbitControls = null!;
  private renderer: THREE.WebGLRenderer = null!;
  private ambientLightIntensity = 2.0;
  private ambientLightLowIntensity = 1.5;
  private directionalLightIntensity = 1.0;
  private ambientLight: THREE.AmbientLight = new THREE.AmbientLight(0xffffff, this.ambientLightIntensity);
  private directionalLight = new THREE.DirectionalLight(0xffffff, this.directionalLightIntensity); // this is for shadows

  private raycaster: THREE.Raycaster = null!;
  private currentRandomRadius = Math.random() * 360;

  // Shader material related
  private rgbBlockTexture?: THREE.Texture;
  private rgbBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockTexture: THREE.Texture | null = null;
  private useRgbTexture = 0.0;
  private highlightedHoldsTexture?: THREE.Texture;

  private currentGltf?: GLTF;
  private initialized = false; // temporary 'fix' for a timing problem

  public constructor() {
    effect(() => {
      const rawModel = this.rawModel();
      if (rawModel !== this.proccessedRawModel) {
        this.proccessedRawModel = rawModel;
        if (rawModel !== undefined) {
          // this effect can run through before afterInit is finished. Needs fixing.
          this.removePreviousAndAddBoulderToScene(rawModel);
        }
      }
    });

    effect(() => {
      const boulderProblem = this.boulderProblem();

      if (boulderProblem) {
        this.setHighlightedHoldsTextureFromData(boulderProblem.image, 128, 128);
        this.ambientLight.intensity = this.ambientLightLowIntensity;
      } else {
        this.highlightedHoldsTexture = undefined;
        this.ambientLight.intensity = this.ambientLightIntensity;
      }
    });

    const activatedRoute = inject(ActivatedRoute);
    this.rgbBlockTexture = activatedRoute.snapshot.data['spraywallDebugTexture'];
  }

  public ngOnInit(): void {
    this.setupHighlightDebugTexture();
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
    this.initialized = true;
  }

  public switchTexture(): void {
    if (this.rgbBlockMaterial && this.originalBlockMaterial && this.currentGltf) {
      this.useRgbTexture = this.useRgbTexture === 0.0 ? 1.0 : 0.0;
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
    this.scene.add(this.directionalLight);
    this.scene.add(this.ambientLight);
    this.controls = new OrbitControls(this.camera, this.renderer.domElement);

    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(1);

    this.loop();
  }

  private loop = () => {

    if (this.rgbBlockMaterial) {
      const shader = this.rgbBlockMaterial.userData['shader'];

      if (shader) {
        shader.uniforms.useRgbTexture.value = this.useRgbTexture;
        shader.uniforms.highlightedHoldsTexture.value = this.highlightedHoldsTexture;
      }
    }

    this.renderer.render(this.scene, this.camera);
    window.requestAnimationFrame(this.loop);
  }

  private removePreviousAndAddBoulderToScene(buffer: ArrayBuffer): void {
    this.loader.parse(buffer, '', (gltf: GLTF) => {
      this.scene.add(gltf.scene);
      // let childCounter = 0;
      gltf.scene.traverse((child) => {
        child.layers.set(1);
        // childCounter++;
        const mesh = (child as THREE.Mesh);
        if (mesh.isMesh) {
          this.originalBlockMaterial = mesh.material as THREE.MeshPhysicalMaterial;
          this.originalBlockTexture = this.originalBlockMaterial.map;
          this.originalBlockMaterial.needsUpdate = true;
          // this.originalBlockTexture!.needsUpdate = true;
          this.originalBlockTexture!.colorSpace = THREE.LinearSRGBColorSpace;
          this.renderer.outputColorSpace = THREE.LinearSRGBColorSpace;
          // this.originalBlockMaterial.wireframe = true;
          this.rgbBlockMaterial = this.setupCustomShaderMaterial();
        }
      });

      if (this.currentGltf !== undefined) {
        this.removeBoulderFromScene(this.currentGltf);
      } else {
        if (this.initialized) {
          fitCameraToCenteredObject(this.camera, gltf.scene, 0, this.controls);
        }
      }
      this.currentGltf = gltf;
      this.setupHighlightTexture(); // we don't know when the model is loaded, so try to swap here (no-op if model not loaded yet)
    },
    (err: ErrorEvent) => {
      throw new Error(err.message);
    });
  }

  private removeBoulderFromScene(gltf: GLTF): void {
    this.scene.remove(gltf.scene);
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

  private setHighlightedHoldsTextureFromData(base64String: string, width: number, height: number): void {
    const image = new Image(width, height);
    const texture = new THREE.Texture(image);
    image.onload = () => {
      texture.flipY = false;
      texture.needsUpdate = true;
      texture.minFilter = THREE.NearestFilter;
      texture.magFilter = THREE.NearestFilter;
      this.highlightedHoldsTexture = texture;
    }
    image.onabort = (ev) => {
      console.error('Failed to load highlighted holds texture from base64 data.', ev);
    }
    image.onerror = (ev) => {
      console.error('Failed to load highlighted holds texture from base64 data.', ev);
    }
    image.src = 'data:image/png;base64,' + base64String;
  }

  private setupHighlightTexture(): void {
    if (this.rgbBlockMaterial && this.originalBlockMaterial && this.currentGltf) {
      const object = (this.currentGltf.scene.children[0] as THREE.Mesh);
      object.material = this.rgbBlockMaterial;
    }
  }

  private setupHighlightDebugTexture() {
    const loader = new THREE.TextureLoader();
    loader.load('./images/highlight_debug_spraywall_test.png', (texture: THREE.Texture) => {
      texture.flipY = false;
      texture.needsUpdate = true;
      texture.minFilter = THREE.NearestFilter;
      texture.magFilter = THREE.NearestFilter;
      this.rgbBlockTexture = texture;
      // this.rgbBlockImageData = getImageDataFromTexture(texture);
      this.rgbBlockMaterial = this.setupCustomShaderMaterial();
      this.setupHighlightTexture(); // we don't know when the model is loaded, so try to swap here (no-op if model not loaded yet)
    });
  }

  private setupCustomShaderMaterial(): THREE.MeshPhysicalMaterial {
    const material = new THREE.MeshPhysicalMaterial({ map: this.originalBlockTexture });

    material.onBeforeCompile = (shader) => {
      shader.uniforms['rgbTexture'] = { value: this.rgbBlockTexture };
      shader.uniforms['time'] = { value: 0 };
      shader.uniforms['useRgbTexture'] = { value: this.useRgbTexture };
      shader.uniforms['highlightedHoldsTexture'] = { value: this.highlightedHoldsTexture };

      shader.vertexShader = shader.vertexShader.replace(
        'varying vec3 vViewPosition;',
        vViewPositionReplace.join('\n')
      );

      shader.vertexShader = shader.vertexShader.replace(
        '#include <begin_vertex>',
        beginVertex.join('\n')
      );

      shader.vertexShader = shader.vertexShader.replace(
        '#include <worldpos_vertex>',
        worldposVertex.join('\n')
      );

      shader.fragmentShader = shader.fragmentShader.replace(
        'uniform float opacity;',
        uniforms.join('\n')
      );

      shader.fragmentShader = shader.fragmentShader.replace(
        '#include <map_fragment>',
        mapFragment.join( '\n' )
      );

      material.userData['shader'] = shader;
    }

    return material;
  }
}


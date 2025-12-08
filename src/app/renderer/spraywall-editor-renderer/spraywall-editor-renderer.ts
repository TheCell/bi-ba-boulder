import { AfterViewInit, ChangeDetectionStrategy, Component, effect, ElementRef, HostListener, inject, input, InputSignal, OnInit, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import * as THREE from 'three';
import { KeyboardShortcutsModule, ShortcutInput } from 'ng-keyboard-shortcuts';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { BoulderLine } from '../../interfaces/boulder-line';
import { fitCameraToCenteredObject } from '../common/camera-utils';
import { beginVertex, mapFragment, uniforms, vViewPositionReplace, worldposVertex } from '../common/shader-code';
import { holdColorOptions, TypeAndColor } from '../common/spraywall-hold-types';
import { ColorAndIndex } from '../common/spraywall-color-and-index';
import { getImageDataFromTexture } from '../common/util';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-spraywall-editor-renderer',
  imports: [
    KeyboardShortcutsModule,
    FormsModule,
    ReactiveFormsModule],
  templateUrl: './spraywall-editor-renderer.html',
  styleUrl: './spraywall-editor-renderer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallEditorRenderer implements OnInit, AfterViewInit {
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

  public rawModel: InputSignal<ArrayBuffer | undefined> = input<ArrayBuffer>();
  public highlightUv: InputSignal<THREE.Texture<HTMLImageElement> | undefined> = input<THREE.Texture<HTMLImageElement>>();
  public highlightColor: InputSignal<THREE.Color> = input.required<THREE.Color>()
  public currentHighlightedHoldsTexturePath: InputSignal<string> = input.required<string>();
  public resetRoute: InputSignal<Subject<void>> = input.required<Subject<void>>();

  public holdColorOptions: TypeAndColor[] = holdColorOptions;
  public shortcuts: ShortcutInput[] = [];

  private proccessedRawModel?: ArrayBuffer;
  private processedLines: BoulderLine[] = [];
  private scene = new THREE.Scene();
  private loader = new GLTFLoader();
  private camera: THREE.PerspectiveCamera = null!;
  private controls: OrbitControls = null!;
  private renderer: THREE.WebGLRenderer = null!;
  private ambientLight: THREE.AmbientLight = new THREE.AmbientLight(0xffffff, 2.0);
  private raycaster: THREE.Raycaster = null!;

  private rgbBlockTexture?: THREE.Texture;
  private rgbBlockImageData?: THREE.DataTextureImageData;
  private rgbBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockTexture: THREE.Texture | null = null;
  private useRgbTexture = 0.0;
  // private currentHighlightedHoldsTexturePath = './images/Bimano_Spraywall_02_highlight_01.png';
  private highlightedHoldsTexture?: THREE.DataTexture;
  private lastClickedHold?: ColorAndIndex;

  private currentGltf?: GLTF;
  private initialized = false; // temporary 'fix' for a timing problem

  public constructor() {
    effect(() => {
      const rawModel = this.rawModel();
      if (rawModel !== this.proccessedRawModel) {
        this.proccessedRawModel = rawModel;
        if (rawModel !== undefined) {
          this.removePreviousAndAddBoulderToScene(rawModel);
        }
      }
    });

    effect(() => {
      const highlightUv = this.highlightUv();
      if (highlightUv) {
        this.setupHighlightDebugTexture(highlightUv);
      }
    });

    effect(() => {
      if (this.highlightColor()) {
        this.lastClickedHold = undefined;
      }
      
    })

    window.addEventListener( 'contextmenu', this.getClickCoordinate.bind(this) );

    // effect(() => {
    //   const selectedId = this.selectedProblemId();
    //   this.ambientLight.intensity = 2.0;

    //   if (selectedId && selectedId.length > 0) {
    //     const problem = this.boulderProblems().find((p) => p.id === selectedId);

    //     if (problem) {
    //       this.setHighlightedHoldsTextureFromData(problem.image);
    //       this.ambientLight.intensity = 0.7;
    //     }
    //   }
    // });
  }

  public ngOnInit(): void {
    // this.setupHighlightDebugTexture();
    this.resetRoute().subscribe({
      next: (t) => {
        console.log(t);
        this.newRoute();
        
      }
    })
    this.newRoute();
  }
  
  public ngAfterViewInit(): void {
    this.createCanvas();

    this.loadHighlightedHoldsTexture(this.currentHighlightedHoldsTexturePath());
    this.initialized = true;
  }

  public switchTexture(): void {
    if (this.rgbBlockMaterial && this.originalBlockMaterial && this.currentGltf) {
      this.useRgbTexture = this.useRgbTexture === 0.0 ? 1.0 : 0.0;
    }
  }
  
  public newRoute(): void {
    const width = 128;
    const height = 128;
    const size = width * height;
    const data = new Uint8Array( size * 4 );
    for ( let i = 0; i < size; i ++ ) {
      const stride = i * 4;
      data[ stride ] = 0;     // red
      data[ stride + 1 ] = 0; // green
      data[ stride + 2 ] = 0; // blue
      data[ stride + 3 ] = 255; // alpha
    }

    const texture = new THREE.DataTexture(data, width, height, THREE.RGBAFormat);
    texture.needsUpdate = true;
    this.highlightedHoldsTexture = texture;
    // this.drawingNewHighlight = true;
  }

  // todo listen to input signal
  public onHoldColorChange(_event: EventTarget | null): void {
    // const selectElement = event as HTMLSelectElement;
    // const selectedColorType = parseInt(selectElement.value);
    // const selectedColorOption = holdColorOptions.find(option => option.type === selectedColorType);
    // if (selectedColorOption) {
    //   this.highlightColor() = selectedColorOption.color;
    //   console.log(`Hold color changed to : ${this.enumName(selectedColorOption.type)} `, this.highlightColor());
    // }
  }
  
  public getRouteImage(): string | undefined {
    if (this.highlightedHoldsTexture?.isTexture && this.highlightedHoldsTexture.image && this.highlightedHoldsTexture.image.data) {
      const canvas = document.createElement('canvas');
      const context = canvas.getContext('2d')!;
      const imgData = context.createImageData(this.highlightedHoldsTexture.image.width, this.highlightedHoldsTexture.image.height);
      canvas.width = this.highlightedHoldsTexture.image.width;
      canvas.height = this.highlightedHoldsTexture.image.height;

      for (let i = 0; i < this.highlightedHoldsTexture.image.data.length; i += 4) {
        imgData.data[i] = this.highlightedHoldsTexture.image.data[i];
        imgData.data[i + 1] = this.highlightedHoldsTexture.image.data[i + 1];
        imgData.data[i + 2] = this.highlightedHoldsTexture.image.data[i + 2];
        imgData.data[i + 3] = 255;
      }

      context.putImageData(imgData, 0, 0);
      return canvas.toDataURL('image/png');
    }

    return undefined;
  }
  
  // todo
  private setupHighlightDebugTexture(texture: THREE.Texture<HTMLImageElement>) {
    // const loader = new THREE.TextureLoader();
    // todo
    // this texture is unique for every spraywall model. It contains the unique B values for the groupings
    // loader.load('./images/Bimano_Spraywall_2025_rgb_blocks_128x128.png', (texture) => {
    //   texture.flipY = false;
    //   texture.needsUpdate = true;
    //   texture.minFilter = THREE.NearestFilter;
    //   texture.magFilter = THREE.NearestFilter;
    //   this.rgbBlockTexture = texture;

      this.rgbBlockImageData = getImageDataFromTexture(texture);
      this.rgbBlockMaterial = this.setupCustomShaderMaterial();
      this.setupHighlightTexture(); // we don't know when the model is loaded, so try to swap here (no-op if model not loaded yet)
    // });
  }
  
  private setupHighlightTexture(): void {
    if (this.rgbBlockMaterial && this.originalBlockMaterial && this.currentGltf) {
      const object = (this.currentGltf.scene.children[0] as THREE.Mesh);
      object.material = this.rgbBlockMaterial;
    }
  }
  
  private loadHighlightedHoldsTexture(path: string): void {
    const loader = new THREE.DataTextureLoader();
    loader.load(path, (texture: THREE.DataTexture) => {
      texture.flipY = false;
      texture.needsUpdate = true;
      texture.minFilter = THREE.NearestFilter;
      texture.magFilter = THREE.NearestFilter;
      this.highlightedHoldsTexture = texture;
    });
  }
  
  // private setHighlightedHoldsTextureFromData(base64String: string): void {
  //   const loader = new THREE.DataTextureLoader();
  //   loader.load('data:image/png;base64,' + base64String, (texture: THREE.DataTexture) => {
  //     texture.flipY = false;
  //     texture.needsUpdate = true;
  //     texture.minFilter = THREE.NearestFilter;
  //     texture.magFilter = THREE.NearestFilter;
  //     this.highlightedHoldsTexture = texture;
  //   });
  // }


  
  private removePreviousAndAddBoulderToScene(buffer: ArrayBuffer): void {
    this.loader.parse(buffer, '', (gltf: GLTF) => {
      this.scene.add(gltf.scene);
      this.rgbBlockMaterial = undefined;
      this.originalBlockMaterial = undefined;
      this.originalBlockTexture = null;

      gltf.scene.traverse((child) => {
        child.layers.set(1);
        const mesh = (child as THREE.Mesh);
        if (mesh.isMesh) {
          this.originalBlockMaterial = mesh.material as THREE.MeshPhysicalMaterial;
          this.originalBlockTexture = this.originalBlockMaterial.map;

          this.originalBlockMaterial.needsUpdate = true;
          if (this.originalBlockTexture) {
            this.originalBlockTexture.colorSpace = THREE.LinearSRGBColorSpace;
          }
          this.renderer.outputColorSpace = THREE.LinearSRGBColorSpace;
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
  
  private getClickCoordinate(event: Event): void {
    
    if (this.scene == undefined || this.scene.children == undefined) {
      return;
    }
    
    const mouseEvent = event as MouseEvent;
    const pointer = new THREE.Vector2();
    const canvasWidth = this.canvas.nativeElement.offsetWidth;
    const canvasHeight = this.canvas.nativeElement.offsetHeight;
    const canvasTop = this.canvas.nativeElement.getBoundingClientRect().top;
    const canvasLeft = this.canvas.nativeElement.getBoundingClientRect().left;

    const mouseX = mouseEvent.clientX - canvasLeft;
    const mouseY = mouseEvent.clientY - canvasTop;

    pointer.x = (mouseX / (canvasWidth)) * 2 - 1;
    pointer.y = - (mouseY / (canvasHeight)) * 2 + 1;

    this.raycaster.setFromCamera(pointer, this.camera);

    const intersects = this.raycaster.intersectObjects(this.scene.children);
    // this.scene.add(new THREE.ArrowHelper(this.raycaster.ray.direction, this.raycaster.ray.origin, 300, 0xff0000));

    if (intersects.length === 0) {
      return;
    }

    if (this.rgbBlockImageData && intersects[0].uv1) {
      this.drawNewHighlight(intersects[0].uv1);
    }
  }
  
  private sampleColorFromImageData(imageData: THREE.DataTextureImageData, u: number, v: number): ColorAndIndex {
    const { data, width, height } = imageData;

    if (!data) {
      throw new Error('Image data is null or undefined.');
    }

    // Clamp and flip Y (because WebGL textures are usually upside down)
    u = THREE.MathUtils.clamp(u, 0, 1);
    v = THREE.MathUtils.clamp(v, 0, 1);

    const x = Math.floor(u * (width - 1));
    const y = Math.floor(v * (height - 1));
    const index = (y * width + x) * 4;

    const r = data[index];
    const g = data[index + 1];
    const b = data[index + 2];
    const a = data[index + 3];

    return { r, g, b, a, index };
  }

  private getIndicesForGroup(imageData: THREE.DataTextureImageData, group: number): number[] {
    const indices: number[] = [];
    const { data } = imageData;

    if (!data) {
      throw new Error('Image data is null or undefined.');
    }

    for (let index = 2; index < data.length; index += 4) {
      const blue = data[index];
      if (blue === group) {
        indices.push(index - 2);
      }
    }

    return indices;
  }

  private drawNewHighlight(uv: THREE.Vector2): void {
    console.log('drawNewHighlight');
    
    if (!this.rgbBlockImageData) {
      return;
    }

    const colorAndIndex = this.sampleColorFromImageData(this.rgbBlockImageData, uv.x, uv.y);
    // console.log(`R=${(colorAndIndex.r).toFixed(0)} G=${(colorAndIndex.g).toFixed(0)} B=${(colorAndIndex.b).toFixed(0)}`);
    if (!this.highlightedHoldsTexture || this.highlightedHoldsTexture.image.data === null) {
      console.error('No highlighted holds texture or data to draw on.');
      return;
    }

    console.log(colorAndIndex.b);
    if (this.lastClickedHold?.index === colorAndIndex.index) {
      const group = this.getIndicesForGroup(this.rgbBlockImageData, colorAndIndex.b);
      let everythingWasHighlighted = true;
      let nothingWasHighlighted = true;

      // eslint-disable-next-line @typescript-eslint/prefer-for-of
      for (let groupIndexIterator = 0; groupIndexIterator < group.length; groupIndexIterator++) {

        if (this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] + this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] + this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] === 0) {
          everythingWasHighlighted = false;
        }
        if (this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] +
            this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] +
            this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] > 0) {
          nothingWasHighlighted = false;
        }

        this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] = this.highlightColor().r;
        this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] = this.highlightColor().g;
        this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] = this.highlightColor().b;
      }

      if (everythingWasHighlighted) {
        // eslint-disable-next-line @typescript-eslint/prefer-for-of
        for (let groupIndexIterator = 0; groupIndexIterator < group.length; groupIndexIterator++) {
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] = 0;
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] = 0;
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] = 0;
        }
      } else if (nothingWasHighlighted) {
        // eslint-disable-next-line @typescript-eslint/prefer-for-of
        for (let groupIndexIterator = 0; groupIndexIterator < group.length; groupIndexIterator++) {
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] = 0;
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] = 0;
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] = 0;
        }

        this.highlightedHoldsTexture!.image.data[colorAndIndex.index] = this.highlightColor().r;
        this.highlightedHoldsTexture!.image.data[colorAndIndex.index + 1] = this.highlightColor().g;
        this.highlightedHoldsTexture!.image.data[colorAndIndex.index + 2] = this.highlightColor().b;
      }
    } else {
      this.highlightedHoldsTexture!.image.data[colorAndIndex.index] = this.highlightColor().r;
      this.highlightedHoldsTexture!.image.data[colorAndIndex.index + 1] = this.highlightColor().g;
      this.highlightedHoldsTexture!.image.data[colorAndIndex.index + 2] = this.highlightColor().b;
    }

    this.highlightedHoldsTexture!.needsUpdate = true;
    this.lastClickedHold = colorAndIndex;
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

  ///
  /// Core Threejs
  ///
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
}

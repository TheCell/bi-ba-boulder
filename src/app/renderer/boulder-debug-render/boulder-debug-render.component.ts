import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, effect, ElementRef, HostListener, inject, input, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import * as THREE from 'three';
import { KeyboardShortcutsModule, ShortcutEventOutput, ShortcutInput } from 'ng-keyboard-shortcuts';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { Line2 } from 'three/examples/jsm/lines/Line2.js';
import { LineMaterial } from 'three/addons/lines/LineMaterial.js';
import { LineGeometry } from 'three/addons/lines/LineGeometry.js';
import { GLTF, GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { BoulderLine } from '../../interfaces/boulder-line';
import { fitCameraToCenteredObject } from '../common/camera-utils';
import { HSLToHex } from '../../utils/color-util';
import { VertexNormalsHelper } from 'three/addons/helpers/VertexNormalsHelper.js';
import Stats from 'stats.js'
import { SpraywallService, SpraywallProblemDto } from '../../api';
import { beginVertex, mapFragment, uniforms, vViewPositionReplace, worldposVertex } from '../common/shader-code';
import { downloadSpraywallProblemImage, getImageDataFromTexture } from '../common/util';
import { ActivatedRoute } from '@angular/router';

interface ColorAndIndex {
  r: number;
  g: number;
  b: number;
  a: number;
  index: number;
}

interface TypeAndIndex {
  type: Type;
  index: number;
}

interface TypeAndColor {
  type: Type;
  color: THREE.Color;
}

interface ITempForm {
  tempPsw: FormControl<string>;
}

enum Type {
  undefined = 0,
  start = 1,
  top = 2,
  hold = 3,
  foot = 4,
  custom = 5
}

@Component({
  selector: 'app-boulder-debug-render',
  imports: [
    CommonModule,
    KeyboardShortcutsModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './boulder-debug-render.component.html',
  styleUrl: './boulder-debug-render.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BoulderDebugRenderComponent implements OnInit, AfterViewInit {
  private spraywallService = inject(SpraywallService);

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
  public blocId = input.required<string>();
  public lines = input<BoulderLine[]>();
  public boulderProblems = input<SpraywallProblemDto[]>([]);
  public selectedProblemId = input<string>();
  public form: FormGroup<ITempForm>;

  private proccessedRawModel?: ArrayBuffer;
  private processedLines: BoulderLine[] = [];
  private scene = new THREE.Scene();
  private loader = new GLTFLoader();
  private camera: THREE.PerspectiveCamera = null!;
  private controls: OrbitControls = null!;
  private renderer: THREE.WebGLRenderer = null!;
  private ambientLight: THREE.AmbientLight = new THREE.AmbientLight(0xffffff, 2.0);

  private raycaster: THREE.Raycaster = null!;
  private currentRandomRadius = Math.random() * 360;

  private clickPoints: THREE.Vector3[] = [];
  private currentLineMaterial: LineMaterial = null!;
  private currentLine: Line2 = new Line2();
  private vertexNormalsHelpers: VertexNormalsHelper[] = [];

  // Shader material related
  private rgbBlockTexture?: THREE.Texture;
  private rgbBlockImageData?: THREE.DataTextureImageData;
  private rgbBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockTexture: THREE.Texture | null = null;
  private useRgbTexture = 0.0;
  private currentHighlightedHoldsTexturePath = './images/Bimano_Spraywall_02_highlight_01.png';
  private highlightedHoldsTexture?: THREE.DataTexture;

  public holdColorOptions: TypeAndColor[] = [
    { type: Type.start, color: new THREE.Color(0, 158, 115) },
    { type: Type.top, color: new THREE.Color(213, 94, 0) },
    { type: Type.hold, color: new THREE.Color(86, 180, 233) },
    { type: Type.foot, color: new THREE.Color(240, 228, 66) },
    { type: Type.custom, color: new THREE.Color(204, 121, 167) }
  ];
  public highlightColor: THREE.Color = this.holdColorOptions[2].color;
  private drawingNewHighlight = false;
  private lastClickedHold?: ColorAndIndex;

  private stats?: Stats;

  private currentGltf?: GLTF;
  private initialized = false; // temporary 'fix' for a timing problem

  public constructor(
    private el: ElementRef) {
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
      const selectedId = this.selectedProblemId();
      this.ambientLight.intensity = 2.0;

      if (selectedId && selectedId.length > 0) {
        const problem = this.boulderProblems().find((p) => p.id === selectedId);

        if (problem) {
          this.setHighlightedHoldsTextureFromData(problem.image, 128, 128);
          this.ambientLight.intensity = 0.7;
        }
      }
    });

    this.shortcuts.push({
      key: ['ctrl + z'],
      preventDefault: true,
      command: (e: ShortcutEventOutput) => this.removeLastPoint() // eslint-disable-line @typescript-eslint/no-unused-vars
    }, {
      key: ['ctrl + space'],
      preventDefault: true,
      command: (e: ShortcutEventOutput) => this.startNewLine() // eslint-disable-line @typescript-eslint/no-unused-vars
    }, {
      key: ['ctrl + y'],
      preventDefault: true,
      command: (e: ShortcutEventOutput) => this.printClickPoints() // eslint-disable-line @typescript-eslint/no-unused-vars
    }, {
      key: ['ctrl + shift'],
      preventDefault: true,
      command: (e: ShortcutEventOutput) => this.toggleNormalLines() // eslint-disable-line @typescript-eslint/no-unused-vars
    });

    window.addEventListener( 'contextmenu', this.getClickCoordinate.bind(this) );

    this.form = new FormGroup<ITempForm>({
      tempPsw: new FormControl<string>('', { nonNullable: true })
    });
  }

  public ngOnInit(): void {
    this.setupHighlightDebugTexture();
  }

  public ngAfterViewInit(): void {
    this.createCanvas();
    this.currentLineMaterial = this.getNewLineMaterial();

    const lines = this.lines();
    if (lines !== this.processedLines) {
      if (lines !== undefined) {
        lines.forEach((line: BoulderLine) => {
          this.addLineToScene(this.scene, line.points.map((point) => new THREE.Vector3(point.x, point.y, point.z)), line.color);
        });

        this.processedLines = lines;
      }
    }

    this.loadHighlightedHoldsTexture(this.currentHighlightedHoldsTexturePath);
    this.initialized = true;
  }

  public switchTexture(): void {
    if (this.rgbBlockMaterial && this.originalBlockMaterial && this.currentGltf) {
      this.useRgbTexture = this.useRgbTexture === 0.0 ? 1.0 : 0.0;
    }
  }

  public switchRoute(): void {
    this.currentHighlightedHoldsTexturePath = this.currentHighlightedHoldsTexturePath === './images/Bimano_Spraywall_02_highlight_02.png' ? './images/Bimano_Spraywall_02_highlight_01.png' : './images/Bimano_Spraywall_02_highlight_02.png';
    this.loadHighlightedHoldsTexture(this.currentHighlightedHoldsTexturePath);
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
    this.drawingNewHighlight = true;
  }

  public downloadRoute(): void {
    if (this.highlightedHoldsTexture) {
      downloadSpraywallProblemImage(this.highlightedHoldsTexture);
    }
  }

  public uploadRoute(): void {
    if (this.highlightedHoldsTexture?.isTexture && this.highlightedHoldsTexture.image && this.highlightedHoldsTexture.image.data) {
      let canvas = document.createElement('canvas');
      let context = canvas.getContext('2d')!;
      let imgData = context.createImageData(this.highlightedHoldsTexture.image.width, this.highlightedHoldsTexture.image.height);
      canvas.width = this.highlightedHoldsTexture.image.width;
      canvas.height = this.highlightedHoldsTexture.image.height;

      for (let i = 0; i < this.highlightedHoldsTexture.image.data.length; i += 4) {
        imgData.data[i] = this.highlightedHoldsTexture.image.data[i];
        imgData.data[i + 1] = this.highlightedHoldsTexture.image.data[i + 1];
        imgData.data[i + 2] = this.highlightedHoldsTexture.image.data[i + 2];
        imgData.data[i + 3] = 255;
      }

      context.putImageData(imgData, 0, 0);
      this.spraywallService.postSpraywallProblemCreate('1', {
        tempPwd: this.form.controls.tempPsw.value,
        name: 'New Problem',
        description: 'Description of the new problem',
        image: canvas.toDataURL('image/png')
      }).subscribe({
        next: (response: SpraywallProblemDto) => {
          console.log('Problem created successfully:', response);
        },
        error: (error: unknown) => {
          console.error('Error creating problem:', error);
        }
      });
    }
  }

  public onHoldColorChange(event: EventTarget | null): void {
    const selectElement = event as HTMLSelectElement;
    const selectedColorType = parseInt(selectElement.value);
    const selectedColorOption = this.holdColorOptions.find(option => option.type === selectedColorType);
    if (selectedColorOption) {
      this.highlightColor = selectedColorOption.color;
      console.log(`Hold color changed to : ${this.enumName(selectedColorOption.type)} `, this.highlightColor);
    }
  }

  public enumName(type: Type): string {
    const enumNames = Object.keys(Type).filter(key => isNaN(Number(key)));
    return enumNames[type];
  }

  public displayStats(): void {
    if (this.stats) {
      return;
    }

    this.stats = new Stats();
    this.stats.showPanel(0); // 0: fps, 1: ms, 2: mb, 3+: custom

    let offset = 10;
    this.stats.dom.style.position = 'absolute';
    this.stats.dom.style.top = `${offset}px`;
    for (let i = 0; i < this.stats.dom.children.length; i++) {
      offset += 50;
      const element = this.stats.dom.children[i] as HTMLElement;
      element.style.position = 'absolute';
      element.style.display = 'block';
      element.style.top = `${offset}px`;
    }

    this.el.nativeElement.appendChild( this.stats.dom );
  }

  private setupHighlightDebugTexture() {
    const loader = new THREE.TextureLoader();
    // this texture is unique for every spraywall model. It contains the unique B values for the groupings
    loader.load('./images/Bimano_Spraywall_2025_rgb_blocks_128x128.png', (texture) => {
      texture.flipY = false;
      texture.needsUpdate = true;
      texture.minFilter = THREE.NearestFilter;
      texture.magFilter = THREE.NearestFilter;
      this.rgbBlockTexture = texture;
      console.log(texture);

      this.rgbBlockImageData = getImageDataFromTexture(texture);
      this.rgbBlockMaterial = this.setupCustomShaderMaterial();
      this.setupHighlightTexture(); // we don't know when the model is loaded, so try to swap here (no-op if model not loaded yet)
    });
  }

  private setupHighlightTexture(): void {
    if (this.rgbBlockMaterial && this.originalBlockMaterial && this.currentGltf) {
      let object = (this.currentGltf.scene.children[0] as THREE.Mesh);
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

  private setHighlightedHoldsTextureFromData(base64String: string, width: number, height: number): void {
    // const image = new Image(width, height);
    const loader = new THREE.DataTextureLoader();
    loader.load('data:image/png;base64,' + base64String, (texture: THREE.DataTexture) => {
      texture.flipY = false;
      texture.needsUpdate = true;
      texture.minFilter = THREE.NearestFilter;
      texture.magFilter = THREE.NearestFilter;
      this.highlightedHoldsTexture = texture;
    });
    // const texture = new THREE.DataTexture(undefined, width, height, THREE.RGBFormat);
    // texture.source

  //   image.onload = () => {
  //     texture.flipY = false;
  //     texture.needsUpdate = true;
  //     texture.minFilter = THREE.NearestFilter;
  //     texture.magFilter = THREE.NearestFilter;
  //     this.highlightedHoldsTexture = texture;
  //   }
  //   image.onabort = (ev) => {
  //     console.error('Failed to load highlighted holds texture from base64 data.', ev);
  //   }
  //   image.onerror = (ev) => {
  //     console.error('Failed to load highlighted holds texture from base64 data.', ev);
  //   }
  //   image.src = 'data:image/png;base64,' + base64String;
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
    this.scene.add(this.ambientLight);
    this.controls = new OrbitControls(this.camera, this.renderer.domElement);

    this.raycaster = new THREE.Raycaster(this.camera.position);
    this.raycaster.layers.set(1);

    this.loop();
  }

  private loop = () => {
    this.stats?.begin();
    if (this.rgbBlockMaterial) {
      const shader = this.rgbBlockMaterial.userData['shader'];

      if (shader) {
        // shader.uniforms.time.value = performance.now() / 1000;
        shader.uniforms.useRgbTexture.value = this.useRgbTexture;
        shader.uniforms.highlightedHoldsTexture.value = this.highlightedHoldsTexture;
      }
    }

    this.renderer.render(this.scene, this.camera);
    this.stats?.end();
    window.requestAnimationFrame(this.loop);
  }

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

    if (!this.drawingNewHighlight) {
      const normal = intersects[0].normal ?? new THREE.Vector3(0, 0, 0);
      const newPoint: THREE.Vector3 = new THREE.Vector3 (
        intersects[0].point.x + 0.1 * normal.x,
        intersects[0].point.y + 0.1 * normal.y,
        intersects[0].point.z + 0.1 * normal.z
      );
      this.clickPoints.push(newPoint);
      console.log('newPoint:',  `(${newPoint.x}, ${newPoint.y}, ${newPoint.z})`);

      if (this.clickPoints.length < 2) {
        return;
      }

      this.drawLineFromActivePoints(this.scene);
    }
  }

  private drawLineFromActivePoints(scene: THREE.Scene): void {
    scene.remove(this.currentLine);
    const geometry = new LineGeometry();
    geometry.setPositions(this.clickPoints.flatMap((value) => [value.x, value.y, value.z]));
    this.currentLine = new Line2(geometry, this.currentLineMaterial);
    scene.add(this.currentLine);
  }

  private addLineToScene(scene: THREE.Scene, points: THREE.Vector3[], color?: string): void {
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

  private printClickPoints(): void {
    console.log(`INSERT INTO line (bloc_id, color, name, identifier) VALUES (${this.blocId()}, "#${this.currentLineMaterial.color.getHexString()}", "${'todo'.appendUniqueId()}", "${'todo'.appendUniqueId()}")
SET @newLineId = (SELECT LAST_INSERT_ID());
INSERT INTO point (line_id, x, y, z) VALUES ${this.clickPoints.map((point) => `(@newLineId, ${point.x}, ${point.y}, ${point.z})`).join(', ')}`);
  }

  private startNewLine(): void {
    this.currentLineMaterial = this.getNewLineMaterial();
    this.clickPoints = [];
    this.currentLine = new Line2();
  }

  private removeLastPoint(): void {
    this.clickPoints.pop();
    this.scene.remove(this.currentLine);
    if (this.clickPoints.length > 2) {
      this.drawLineFromActivePoints(this.scene);
    }
  }

  private getRandomColor(): string {
    const currentRadius =  this.currentRandomRadius;
    const randomColor = HSLToHex({ h: currentRadius, s: 70, l: 80});
    this.currentRandomRadius *= Math.E;
    this.currentRandomRadius %= 360;
    return randomColor;
  }

  private toggleNormalLines(): void {
    if (this.currentGltf === undefined) {
      return;
    }

    if (this.vertexNormalsHelpers.length > 0) {
      this.scene.remove( ...this.vertexNormalsHelpers );
      this.vertexNormalsHelpers = [];
      return;
    }

    this.currentGltf.scene.traverse((child) => {
      if (child.type === 'Mesh') {
        const vnh = new VertexNormalsHelper( child, 1, 0xff0000 );
        this.vertexNormalsHelpers.push(vnh);
      }
    });

    this.scene.add( ...this.vertexNormalsHelpers );
  }

  private dumpObject(obj: THREE.Group<THREE.Object3DEventMap>, lines: string[] = [], isLast = true, prefix = '') {
    const localPrefix = isLast ? '└─' : '├─';
    lines.push(`${prefix}${prefix ? localPrefix : ''}${obj.name || '*no-name*'} [${obj.type}]`);
    const newPrefix = prefix + (isLast ? '  ' : '│ ');
    const lastNdx = obj.children.length - 1;
    obj.children.forEach((child, ndx) => {
      const isLast = ndx === lastNdx;
      this.dumpObject(child as THREE.Group<THREE.Object3DEventMap>, lines, isLast, newPrefix);
    });
    return lines;
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
    const { data, width, height } = imageData;

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
    if (!this.drawingNewHighlight || !this.rgbBlockImageData) {
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
      for (let groupIndexIterator = 0; groupIndexIterator < group.length; groupIndexIterator++) {

        if (this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] + this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] + this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] === 0) {
          everythingWasHighlighted = false;
        }
        if (this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] +
            this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] +
            this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] > 0) {
          nothingWasHighlighted = false;
        }

        this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] = this.highlightColor.r;
        this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] = this.highlightColor.g;
        this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] = this.highlightColor.b;
      }

      if (everythingWasHighlighted) {
        for (let groupIndexIterator = 0; groupIndexIterator < group.length; groupIndexIterator++) {
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] = 0;
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] = 0;
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] = 0;
        }
      } else if (nothingWasHighlighted) {
        for (let groupIndexIterator = 0; groupIndexIterator < group.length; groupIndexIterator++) {
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator]] = 0;
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 1] = 0;
          this.highlightedHoldsTexture!.image.data[group[groupIndexIterator] + 2] = 0;
        }

        this.highlightedHoldsTexture!.image.data[colorAndIndex.index] = this.highlightColor.r;
        this.highlightedHoldsTexture!.image.data[colorAndIndex.index + 1] = this.highlightColor.g;
        this.highlightedHoldsTexture!.image.data[colorAndIndex.index + 2] = this.highlightColor.b;
      }
    } else {
      this.highlightedHoldsTexture!.image.data[colorAndIndex.index] = this.highlightColor.r;
      this.highlightedHoldsTexture!.image.data[colorAndIndex.index + 1] = this.highlightColor.g;
      this.highlightedHoldsTexture!.image.data[colorAndIndex.index + 2] = this.highlightColor.b;
    }

    this.highlightedHoldsTexture!.needsUpdate = true;
    this.lastClickedHold = colorAndIndex;
  }

  private getBitsFromNumber(type: Type, index: number): number {
    // saving the type in the high endian 2 bits, and the index in the low endian 14 bits
    let twoByteInfo = 0x00000;
    twoByteInfo = (type << 16) | (index);
    return twoByteInfo;
  }

  private dec2bin24(num: number): string {
    // shift value 0 bits to the right
    return (num >>> 0).toString(2).padStart(24, '0');
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

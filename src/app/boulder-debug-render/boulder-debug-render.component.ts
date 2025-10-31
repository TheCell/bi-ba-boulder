import { CommonModule } from '@angular/common';
import { AfterViewInit, ChangeDetectionStrategy, Component, effect, ElementRef, HostListener, input, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { KeyboardShortcutsModule, ShortcutEventOutput, ShortcutInput } from 'ng-keyboard-shortcuts';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { Line2 } from 'three/examples/jsm/lines/Line2.js';
import { LineMaterial } from 'three/addons/lines/LineMaterial.js';
import { LineGeometry } from 'three/addons/lines/LineGeometry.js';
import { GLTFLoader, GLTF } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { BoulderLine } from '../interfaces/boulder-line';
import { fitCameraToCenteredObject } from '../utils/camera-utils';
import { HSLToHex } from '../utils/color-util';
import { VertexNormalsHelper } from 'three/addons/helpers/VertexNormalsHelper.js';
import Stats from 'stats.js'

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
    KeyboardShortcutsModule
  ],
  templateUrl: './boulder-debug-render.component.html',
  styleUrl: './boulder-debug-render.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BoulderDebugRenderComponent implements AfterViewInit {
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
  public blocId = input.required<string>();
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

  private clickPoints: THREE.Vector3[] = [];
  private currentLineMaterial: LineMaterial = null!;
  private currentLine: Line2 = new Line2();
  private vertexNormalsHelpers: VertexNormalsHelper[] = [];

  private rgbBlockTexture?: THREE.Texture;
  private rgbBlockImageData?: ImageData;
  private rgbBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockMaterial?: THREE.MeshPhysicalMaterial;
  private originalBlockTexture: THREE.Texture | null = null;
  private useRgbTexture = 0.0;
  private currentHighlightedHoldsTexturePath = './images/Bimano_Spraywall_02_highlight_01.png';
  private highlightedHoldsTexture?: THREE.Texture;

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
  }

  public ngAfterViewInit(): void {
    this.createCanvas();
    this.addStats();

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

    const loader = new THREE.TextureLoader();
    loader.load('./api-test/boulder/spraywall2/Bimano_Spraywall_02_LOD0_UV.png', (texture: THREE.Texture) => {
      texture.flipY = false;
      texture.needsUpdate = true;
      texture.minFilter = THREE.NearestFilter;
      texture.magFilter = THREE.NearestFilter;
      this.rgbBlockTexture = texture;
      this.rgbBlockImageData = this.getImageDataFromTexture(texture);
      this.rgbBlockMaterial = this.getCustomShaderMaterial();
      
      this.swapTexture();
    });

    this.loadHighlightedHoldsTexture(this.currentHighlightedHoldsTexturePath);
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
      
      let img = new Image();
      img.src = canvas.toDataURL('image/png');
      const link = document.createElement('a');
      link.href = img.src;
      link.download = `highlighted_route_${Date.now()}.png`;
      link.click();
      URL.revokeObjectURL(link.href);

      console.log(canvas.toDataURL('image/png'));
      

      // let timer1a = performance.now();
      // let customRouteData: TypeAndIndex[] = [];
      // for (let i = 0; i < this.highlightedHoldsTexture.image.data.length; i += 4) {
      //   if (this.highlightedHoldsTexture.image.data[i] > 0 || this.highlightedHoldsTexture.image.data[i + 1] > 0 || this.highlightedHoldsTexture.image.data[i + 2] > 0) {          
      //     customRouteData.push({
      //       type: Type.hold,
      //       index: i
      //     });
      //   }
      // }
      // // console.log(customImage);
      // let timer1b = performance.now();
      // console.log('Custom image processing time:', timer1b - timer1a);


      // this is less efficient and won't save data if many holds are selected.
      let indexAndType16BitArray: number[] = [];
      let binaryString = '';
      for (let i = 0; i < this.highlightedHoldsTexture.image.data.length; i += 4) {
        if (this.highlightedHoldsTexture.image.data[i] > 0 || this.highlightedHoldsTexture.image.data[i + 1] > 0 || this.highlightedHoldsTexture.image.data[i + 2] > 0) {          
          const type = this.holdColorOptions.find((typeAndColor: TypeAndColor) => {
            if (typeAndColor.color.r === this.highlightedHoldsTexture!.image.data[i] &&
              typeAndColor.color.g === this.highlightedHoldsTexture!.image.data[i + 1] &&
              typeAndColor.color.b === this.highlightedHoldsTexture!.image.data[i + 2]) {
              return true;
            }

            return false;
          });

          if (type !== undefined) {
            // console.log(type.type);
            let twoByteInfo = this.getBitsFromNumber(type.type, i);
            // console.log(twoByteInfo, this.dec2bin24(twoByteInfo));
            binaryString += this.dec2bin24(twoByteInfo);
            
            indexAndType16BitArray.push(twoByteInfo);
          }
        }
      }

      const binLink = document.createElement('a');
      binLink.href = URL.createObjectURL(new Blob([binaryString], { type: 'application/octet-stream' }));
      binLink.download = `highlighted_route_${Date.now()}.bin`;
      binLink.click();
      URL.revokeObjectURL(binLink.href);
      // console.log(indexAndType16BitArray, binaryString);
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

  private swapTexture(): void {
    if (this.rgbBlockMaterial && this.originalBlockMaterial && this.currentGltf) {
      let object = (this.currentGltf.scene.children[0] as THREE.Mesh);
      object.material = this.rgbBlockMaterial;
    }
  }

  private loadHighlightedHoldsTexture(path: string): void {
    const loader = new THREE.TextureLoader();
    loader.load(path, (texture: THREE.Texture) => {
      texture.flipY = false;
      texture.needsUpdate = true;
      texture.minFilter = THREE.NearestFilter;
      texture.magFilter = THREE.NearestFilter;
      this.highlightedHoldsTexture = texture;
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
      console.log(this.dumpObject(gltf.scene).join('\n'));
      console.log(gltf.scene);
      this.rgbBlockMaterial = undefined;
      this.originalBlockMaterial = undefined;
      this.originalBlockTexture = null;

      gltf.scene.traverse((child) => {
        child.layers.set(1);
        const mesh = (child as THREE.Mesh);
        if (mesh.isMesh) {
          this.originalBlockMaterial = mesh.material as THREE.MeshPhysicalMaterial;
          this.originalBlockTexture = this.originalBlockMaterial.map;
          this.rgbBlockMaterial = this.getCustomShaderMaterial();
        }
      });

      if (this.currentGltf !== undefined) {
        this.removeBoulderFromScene(this.currentGltf);
      } else {
        fitCameraToCenteredObject(this.camera, gltf.scene, 0, this.controls);
      }
      this.currentGltf = gltf;
      this.swapTexture();
    },
    (err: ErrorEvent) => {
      throw new Error(err.message);
    });
  }

  private removeBoulderFromScene(gltf: GLTF): void {
    this.scene.remove(gltf.scene);
  }

  private addLights(scene: THREE.Scene): void {
    const ambientLight = new THREE.AmbientLight(0xffffff, 1);
    scene.add(ambientLight);
  }

  private getClickCoordinate(event: Event): void {
    if (this.scene == undefined || this.scene.children == undefined) {
      return;
    }

    const mouseEvent = event as MouseEvent;
    const pointer = new THREE.Vector2();

    pointer.x = (mouseEvent.clientX / this.canvas.nativeElement.offsetWidth) * 2 - 1;
    pointer.y = - (mouseEvent.clientY / this.canvas.nativeElement.offsetHeight) * 2 + 1;

    this.raycaster.setFromCamera(pointer, this.camera);

    const intersects = this.raycaster.intersectObjects(this.scene.children);
    // this.scene.add(new THREE.ArrowHelper(this.raycaster.ray.direction, this.raycaster.ray.origin, 300, 0xff0000));

    if (intersects.length === 0) {
      return;
    }

    // console.log('intersects:', intersects[0].uv);
    // console.log('intersects:', intersects[0].uv1);

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

  private addStats(): void {
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

    document.body.appendChild( this.stats.dom );
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

  private getImageDataFromTexture(texture: THREE.Texture): ImageData {
    const image = texture.image;
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    
    canvas.width = image.width;
    canvas.height = image.height;

    ctx!.drawImage(image, 0, 0);
    const imageData = ctx!.getImageData(0, 0, image.width, image.height);

    console.log('getImageDataFromTexture', imageData);
    
    return imageData;
  }

  private sampleColorFromImageData(imageData: ImageData, u: number, v: number): ColorAndIndex {
    const { data, width, height } = imageData;

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

  private getIndicesForGroup(imageData: ImageData, group: number): number[] {
    const indices: number[] = [];
    const { data, width, height } = imageData;

    for (let index = 2; index < data.length; index += 4) {
      const blue = data[index];
      if (blue === group) {
        indices.push(index - 2);
      }
    }

    return indices;
  }

  private getCustomShaderMaterial(): THREE.MeshPhysicalMaterial {
    const material = new THREE.MeshPhysicalMaterial({ map: this.originalBlockTexture });

    material.onBeforeCompile = (shader) => {
      shader.uniforms['rgbTexture'] = { value: this.rgbBlockTexture };
      shader.uniforms['time'] = { value: 0 };
      shader.uniforms['useRgbTexture'] = { value: this.useRgbTexture };
      shader.uniforms['highlightedHoldsTexture'] = { value: this.highlightedHoldsTexture };

      shader.vertexShader = shader.vertexShader.replace(
        'varying vec3 vViewPosition;',
        [
          'varying vec3 vViewPosition;',
          'attribute vec2 uv1;',
          'uniform float time;',
          'varying vec2 vUv1;',
          'varying float fresnel;'
        ].join('\n')
      );

      shader.vertexShader = shader.vertexShader.replace(
        '#include <begin_vertex>',
        [
          '#include <begin_vertex>',
          'vUv1 = vec3( uv1, 1 ).xy;',
          // `float theta = 1.0 + sin( time ) / ${ 1.1.toFixed(1) };`,
          // 'transformed.x *= theta;',
        ].join('\n')
      );

      shader.vertexShader = shader.vertexShader.replace(
        '#include <worldpos_vertex>',
        [
          '#if defined( USE_ENVMAP ) || defined( DISTANCE ) || defined ( USE_SHADOWMAP ) || defined ( USE_TRANSMISSION ) || NUM_SPOT_LIGHT_COORDS > 0',
          '	vec4 worldPosition = vec4( transformed, 1.0 );',
          '	#ifdef USE_BATCHING',
          '		worldPosition = batchingMatrix * worldPosition;',
          '	#endif',
          '	#ifdef USE_INSTANCING',
          '		worldPosition = instanceMatrix * worldPosition;',
          '	#endif',
          '	worldPosition = modelMatrix * worldPosition;',
          '#endif',
          // 'fresnel = abs(dot(normalize(vViewPosition), normal));',
          // 'fresnel = dot(normalize(vViewPosition), normal);',
          'float amount = 0.5;',
          'fresnel = pow((1.0 - clamp(dot(normalize(normal), normalize(vViewPosition)), 0.0, 1.0)), amount);',
          // 'fresnel = normal.z;',
          // 'fresnel = dot( normalize( vViewPosition ), normal );',
          // 'fresnel = dot( normalize( vViewPosition ), normal );',
          // 'fresnel = dot(normalize(vViewPosition), normal);',
        ].join('\n')
      );

      shader.fragmentShader = shader.fragmentShader.replace(
        'uniform float opacity;',
        [
          'uniform float opacity;',
          'uniform float useRgbTexture;',
          'uniform sampler2D rgbTexture;',
          'uniform sampler2D highlightedHoldsTexture;',
          'varying vec2 vUv1;',
          'varying float fresnel;'
        ].join('\n')
      );

      shader.fragmentShader = shader.fragmentShader.replace(
        '#include <map_fragment>',
        [
          '#ifdef USE_MAP',
          'vec4 sampledDiffuseColor = useRgbTexture > 0.0 ? texture2D( rgbTexture, vUv1 ) : texture2D( map, vMapUv );',
          'vec4 highlightedHoldsColor = texture2D( highlightedHoldsTexture, vUv1 );',
          '#ifdef DECODE_VIDEO_TEXTURE',
          'sampledDiffuseColor = sRGBTransferEOTF( sampledDiffuseColor );',
          '#endif',
          'diffuseColor *= sampledDiffuseColor;',
          'float hasHighlight = step(0.0, highlightedHoldsColor.r + highlightedHoldsColor.g + highlightedHoldsColor.b);',
          'hasHighlight = clamp(hasHighlight * (1.0 - step(0.5, useRgbTexture)), 0.0, 1.0);',
          'vec3 sampledGray = vec3((sampledDiffuseColor.r + sampledDiffuseColor.g + sampledDiffuseColor.b) / 3.0);',
          'vec3 baseColor = diffuseColor.rgb * (1.0 - fresnel) + sampledGray * fresnel;',
          'vec3 highlightColor = highlightedHoldsColor.rgb * fresnel;',
          'totalEmissiveRadiance.rgb = mix(totalEmissiveRadiance.rgb, highlightColor, hasHighlight);',
          'diffuseColor.rgb = mix(diffuseColor.rgb, diffuseColor.rgb * vec3(1.0 - fresnel), hasHighlight);',
          // 'if (useRgbTexture <= 0.0 && (highlightedHoldsColor.r > 0.0 || highlightedHoldsColor.g > 0.0 || highlightedHoldsColor.b > 0.0)) {',
          // 'vec3 sampledGray = vec3((sampledDiffuseColor.r + sampledDiffuseColor.g + sampledDiffuseColor.b) / 3.0);',
          // 'diffuseColor.rgb = diffuseColor.rgb * (1.0 - fresnel) + sampledGray * fresnel;',
          // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * fresnel;',
          // 'diffuseColor.rgb = diffuseColor.rgb * vec3(1.0 - fresnel);',
          // '',
          // 'float grid_position = rand( vMapUv.xy );',
          // 'float grid_position = clamp(rand( vMapUv.xy ) - 0.5, 0.0, 1.0);',
          // 'vec3 truecolor = vec3(highlightedHoldsColor.r * grid_position + diffuseColor.r * (1.0 - grid_position), highlightedHoldsColor.g * grid_position + diffuseColor.g * (1.0 - grid_position), highlightedHoldsColor.b * grid_position + diffuseColor.b * (1.0 - grid_position));',
          // 'diffuseColor.rgb = truecolor;',
          // 'totalEmissiveRadiance  = truecolor;',
          // 'vec3 mixedHighlight = vec3(highlightedHoldsColor.r * grid_position, highlightedHoldsColor.g * grid_position, highlightedHoldsColor.b * grid_position);',
          // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * sampledGray;',
          // 'float normaliedFresnel = clamp( (fresnel - 0.5) * 2.0, 0.0, 1.0);',
          // 'float clampedFresnel = clamp(fresnel, 0.0, 1.0);',
          // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * (1.0 - fresnel) * 0.5;',
          // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * (1.0 - clampedFresnel) * 0.5;',
          // 'totalEmissiveRadiance.rgb = vec3(0.0);',
          // 'diffuseColor.rgb = vec3(fresnel);',
          // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * (1.0 - fresnel);',
          // 'totalEmissiveRadiance.rgb = vec3(1.0 - fresnel);',
          // 'totalEmissiveRadiance.rgb = vec3(fresnel);',
          // 'diffuseColor.rgb = vec3(1.0 - fresnel);',
          // 'diffuseColor.rgb = diffuseColor.rgb * vec3(1.0 - fresnel);',
          // 'diffuseColor.rgb = highlightedHoldsColor.rgb;',
          // 'diffuseColor.rgb *= vec3(2.0);',
          // '}',
          '#endif'
        ].join( '\n' )
      );

      // shader.fragmentShader = shader.fragmentShader.replace(
      //   '#include <emissivemap_fragment>',
      //   [
      //     '#ifdef USE_EMISSIVEMAP',
      //       'vec4 emissiveColor = texture2D( emissiveMap, vEmissiveMapUv );',
      //       '#ifdef DECODE_VIDEO_TEXTURE_EMISSIVE',
      //         '// use inline sRGB decode until browsers properly support SRGB8_ALPHA8 with video textures (#26516)',
      //         'emissiveColor = sRGBTransferEOTF( emissiveColor );',
      //       '#endif',
      //       'totalEmissiveRadiance *= emissiveColor.rgb;',
      //       '#endif',
      //     'totalEmissiveRadiance.rgb += highlightedHoldsColor.rgb * vec3(0.5);',
      //   ].join( '\n' )
      // );

      material.userData['shader'] = shader;
    }

    return material;
  }

  private drawNewHighlight(uv: THREE.Vector2): void {
    if (!this.drawingNewHighlight || !this.rgbBlockImageData) {
      return;
    }
    
    const colorAndIndex = this.sampleColorFromImageData(this.rgbBlockImageData, uv.x, uv.y);
    console.log(`R=${(colorAndIndex.r).toFixed(0)} G=${(colorAndIndex.g).toFixed(0)} B=${(colorAndIndex.b).toFixed(0)}`);

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
}

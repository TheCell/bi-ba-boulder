import { ChangeDetectionStrategy, Component, inject, OnInit, ViewChild } from '@angular/core';
import { LoadingImageComponent } from 'src/app/common/loading-image/loading-image.component';
import { SpraywallsService } from '@api/index';
import { BoulderLoaderService } from 'src/app/background-loading/boulder-loader.service';
import { SpraywallEditorRenderer } from 'src/app/renderer/spraywall-editor-renderer/spraywall-editor-renderer';
import { holdColorOptions, SpraywallHoldType, TypeAndColor } from 'src/app/renderer/common/spraywall-hold-types';
import * as THREE from 'three';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NonNullableFormBuilder } from '@angular/forms';
import { NgClass } from '@angular/common';
import { Subject } from 'rxjs';
import { Modal } from 'src/app/core/modal/modal/modal';
import { ModalService } from 'src/app/core/modal/modal.service';
import { SpraywallSaveDialog } from '../spraywall-save-dialog/spraywall-save-dialog';
import { SpraywallSaveData } from '../spraywall-save-dialog/spraywall-save-data.interface';
import { ActivatedRoute } from '@angular/router';

interface iHoldColorForm { 
    spraywallHoldType: SpraywallHoldType;
}

@Component({
  selector: 'app-spraywall-editor',
  imports: [LoadingImageComponent, SpraywallEditorRenderer, FormsModule, ReactiveFormsModule, NgClass, Modal],
  templateUrl: './spraywall-editor.html',
  styleUrl: './spraywall-editor.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class SpraywallEditor implements OnInit {
  @ViewChild('modal') private modal!: Modal;
  @ViewChild('renderer') private renderer!: SpraywallEditorRenderer;

  private modalService = inject(ModalService);
  private _fb = inject(NonNullableFormBuilder);
  private spraywallsService = inject(SpraywallsService);
  private boulderLoaderService = inject(BoulderLoaderService);

  public colorForm = this._fb.group<iHoldColorForm>({
    spraywallHoldType: (SpraywallHoldType.start),
  });
  public colorFormId = ''.appendUniqueId();

  public currentRawModel?: ArrayBuffer;
  public currentHighlightUv?: THREE.Texture<HTMLImageElement>;
  public currentHoldColor: THREE.Color = null!;
  public resetSignal: Subject<void> = new Subject<void>();

  public spraywallId = '';
  // public selectedProblem?: SpraywallProblemDto = undefined;
  public holdColorOptions: TypeAndColor[] = holdColorOptions;
  public currentHighlightedHoldsTexturePath = './images/Bimano_Spraywall_02_highlight_01.png';
  
  public constructor() {
    const router = inject(ActivatedRoute);
    this.spraywallId = router.snapshot.paramMap.get('id') ?? '';
    this.currentHoldColor = this.holdColorOptions[0].color;

    this.colorForm.controls.spraywallHoldType.valueChanges.subscribe({
      next: (value) => {
        console.log(value);
        this.currentHoldColor = holdColorOptions[value - 1].color;
      }
    });

    // this.colorForm.controls.spraywallHoldType.setValue(this.currentHoldColor);
  }

  public ngOnInit() {
    // todo cache when switching from spraywall
    this.boulderLoaderService.loadTestSpraywall3().subscribe({
      next: (data: ArrayBuffer) => {
        this.currentRawModel = data;
      }
    });

    const todo = './images/Bimano_Spraywall_2025_rgb_blocks_128x128.png';
    this.loadCustomUv(todo);
  }

  public openSaveModal(): void {
    // console.log(this.renderer.getRouteImage());
    
    const component = this.modalService.open(this.modal.id, SpraywallSaveDialog);
    if (!component) {
      throw new Error('Modal component not found');
    }

    const imageData = this.renderer.getRouteImage();
    if (!imageData) {
      throw new Error('No image data from renderer');
    }

    const dialogData: SpraywallSaveData = { imageData: imageData, spraywallId: this.spraywallId };
    component.initialize!(dialogData);
  }
  
  public enumName(type: SpraywallHoldType): string {
    const enumNames = Object.keys(SpraywallHoldType).filter(key => isNaN(Number(key)));
    return enumNames[type];
  }

  public onHoldColorChange(event: EventTarget | null): void {
    const selectElement = event as HTMLSelectElement;
    const selectedColorType = parseInt(selectElement.value);
    const selectedColorOption = holdColorOptions.find(option => option.type === selectedColorType);
    if (selectedColorOption) {
      this.currentHoldColor = selectedColorOption.color;
    }
  }

  private loadCustomUv(uvPath: string): void {
    const loader = new THREE.TextureLoader();
    loader.load(uvPath, (texture: THREE.Texture<HTMLImageElement>) => {
      texture.flipY = false;
      texture.needsUpdate = true;
      texture.minFilter = THREE.NearestFilter;
      texture.magFilter = THREE.NearestFilter;
      this.currentHighlightUv = texture;
    });
  }
}

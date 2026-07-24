import { ChangeDetectorRef, Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { SpraywallProblemDto, SpraywallsService } from '@api-net/index';
import * as THREE from 'three';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NonNullableFormBuilder } from '@angular/forms';
import { NgClass } from '@angular/common';
import { Subject, Subscription } from 'rxjs';
import { SpraywallSaveDialog } from '../spraywall-save-dialog/spraywall-save-dialog';
import { SpraywallSaveData } from '../spraywall-save-dialog/spraywall-save-data.interface';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { ModalService } from '../../core/modal/modal.service';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { Modal } from '../../core/modal/modal/modal';
import { SpraywallHoldType, TypeAndColor, holdColorOptions } from '../../renderer/common/spraywall-hold-types';
import { SpraywallEditorRenderer } from '../../renderer/spraywall-editor-renderer/spraywall-editor-renderer';
import { CameraControls } from '../../render-overlays/camera-controls/camera-controls';

interface iHoldColorForm {
  spraywallHoldType: SpraywallHoldType;
}

@Component({
  selector: 'app-spraywall-editor',
  imports: [
    LoadingImageComponent,
    SpraywallEditorRenderer,
    FormsModule,
    ReactiveFormsModule,
    NgClass,
    Modal,
    RouterLink,
    CameraControls
  ],
  templateUrl: './spraywall-editor.html',
  styleUrl: './spraywall-editor.scss'
})
export class SpraywallEditor implements OnInit, OnDestroy {
  @ViewChild('modal') private modal!: Modal;
  @ViewChild('renderer') private renderer!: SpraywallEditorRenderer;

  private modalService = inject(ModalService);
  private _fb = inject(NonNullableFormBuilder);
  private spraywallsService = inject(SpraywallsService);
  private boulderLoaderService = inject(BoulderLoaderService);
  private changeDetectorRef = inject(ChangeDetectorRef);
  private router = inject(Router);

  public colorForm = this._fb.group<iHoldColorForm>({
    spraywallHoldType: SpraywallHoldType.hold
  });
  public colorFormId = ''.appendUniqueId();

  public currentRawModel?: ArrayBuffer;
  public currentHighlightUv?: THREE.Texture<HTMLImageElement>;
  public currentHoldColor: THREE.Color = null!;
  public resetSignal: Subject<void> = new Subject<void>();
  public undoLastHighlightSignal: Subject<void> = new Subject<void>();
  public spraywallId = '';
  public problemId? = '';
  public spraywallProblemForEdit?: SpraywallProblemDto;
  public holdColorOptions: TypeAndColor[] = holdColorOptions;

  private subscription = new Subscription();

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.spraywallId = activatedRoute.snapshot.paramMap.get('spraywallId') ?? '';
    this.problemId = activatedRoute.snapshot.paramMap.get('problemId') ?? undefined;
    this.spraywallProblemForEdit = activatedRoute.snapshot.data['spraywallProblem'];

    this.currentHoldColor = this.holdColorOptions[this.colorForm.controls.spraywallHoldType.value - 1].color;

    this.colorForm.controls.spraywallHoldType.valueChanges.subscribe({
      next: (value) => {
        this.currentHoldColor = holdColorOptions[value - 1].color;
      }
    });

    // this.subscription.add(
    //   this.resetSignal.subscribe({
    //     next: () => {
    //       this.problemId = undefined;
    //       this.spraywallProblemForEdit = undefined;
    //     }
    //   })
    // );
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public ngOnInit() {
    // todo cache when switching from spraywall
    this.boulderLoaderService.loadTestSpraywall3().subscribe({
      next: (data: ArrayBuffer) => {
        this.currentRawModel = data;
        this.changeDetectorRef.markForCheck();
      }
    });

    // const todo = './images/Bimano_Spraywall_2025_rgb_blocks_128x128.png';
    const todo = './api-test/boulder/spraywall2/Bimano_Spraywall_2025_rgb_blocks_128x128.png';
    this.loadCustomUv(todo);
  }

  public closeModal(closeModalEvent: CloseModalEvent) {
    if (closeModalEvent.closeType > 0) {
      // don't reset
    } else {
      this.router.navigate(['/', 'spraywall', this.spraywallId]);
    }
  }

  public openSaveModal(): void {
    const component = this.modalService.open(this.modal.id, SpraywallSaveDialog);
    if (!component) {
      throw new Error('Modal component not found');
    }

    const imageData = this.renderer.getRouteImage();
    if (!imageData) {
      throw new Error('No image data from renderer');
    }

    const dialogData: SpraywallSaveData = {
      imageData: imageData,
      spraywallId: this.spraywallId,
      name: ''
    };
    if (this.spraywallProblemForEdit) {
      dialogData.existingId = this.spraywallProblemForEdit.id;
      dialogData.name = this.spraywallProblemForEdit.name;
      dialogData.description = this.spraywallProblemForEdit.description;
      dialogData.fontGrade = this.spraywallProblemForEdit.fontGrade;
      dialogData.isCircuit = this.spraywallProblemForEdit.isCircuit;
      dialogData.noMatch = this.spraywallProblemForEdit.noMatch;
      dialogData.freeFeet = this.spraywallProblemForEdit.freeFeet;
      dialogData.version = this.spraywallProblemForEdit.version;
    }
    component.initialize!(dialogData);
  }

  public enumName(type: SpraywallHoldType): string {
    const enumNames = Object.keys(SpraywallHoldType).filter((key) => isNaN(Number(key)));
    return enumNames[type];
  }

  public onHoldColorChange(event: EventTarget | null): void {
    const selectElement = event as HTMLSelectElement;
    const selectedColorType = parseInt(selectElement.value);
    const selectedColorOption = holdColorOptions.find((option) => option.type === selectedColorType);
    if (selectedColorOption) {
      this.currentHoldColor = selectedColorOption.color;
    }
  }

  private loadCustomUv(uvPath: string): void {
    const loader = new THREE.TextureLoader();
    loader.load(uvPath, (texture: THREE.Texture<HTMLImageElement>) => {
      this.setCustomUvTexture(texture);
    });
  }

  private setCustomUvTexture(texture: THREE.Texture<HTMLImageElement>): void {
    texture.flipY = false;
    texture.needsUpdate = true;
    texture.minFilter = THREE.NearestFilter;
    texture.magFilter = THREE.NearestFilter;
    this.currentHighlightUv = texture;
  }
}

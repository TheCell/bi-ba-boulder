import { ChangeDetectorRef, Component, computed, inject, OnInit, signal, ViewChild } from '@angular/core';
import { SpraywallProblemDto } from '@api-net/index';
import * as THREE from 'three';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { SpraywallSaveDialog } from '../spraywall-save-dialog/spraywall-save-dialog';
import { SpraywallSaveData } from '../spraywall-save-dialog/spraywall-save-data.interface';
import { ActivatedRoute, Router } from '@angular/router';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { ModalService } from '../../core/modal/modal.service';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { Modal } from '../../core/modal/modal/modal';
import { SpraywallHoldType, TypeAndColor, holdColorOptions } from '../../renderer/common/spraywall-hold-types';
import { SpraywallEditorRenderer } from '../../renderer/spraywall-editor-renderer/spraywall-editor-renderer';
import { CameraControls } from '../../render-overlays/camera-controls/camera-controls';
import { form, FormField } from '@angular/forms/signals';

interface IHoldColorForm {
  spraywallHoldType: string;
}

@Component({
  selector: 'app-spraywall-editor',
  imports: [
    LoadingImageComponent,
    SpraywallEditorRenderer,
    FormsModule,
    ReactiveFormsModule,
    FormField,
    Modal,
    CameraControls
  ],
  templateUrl: './spraywall-editor.html',
  styleUrl: './spraywall-editor.scss'
})
export class SpraywallEditor implements OnInit {
  @ViewChild('modal') private modal!: Modal;
  @ViewChild('renderer') private renderer!: SpraywallEditorRenderer;

  private modalService = inject(ModalService);
  private boulderLoaderService = inject(BoulderLoaderService);
  private changeDetectorRef = inject(ChangeDetectorRef);
  private router = inject(Router);

  public colorModel = signal<IHoldColorForm>({
    spraywallHoldType: SpraywallHoldType.hold.toString()
  });
  public colorForm = form(this.colorModel);
  public colorFormId = ''.appendUniqueId();

  public currentRawModel?: ArrayBuffer;
  public currentHighlightUv = signal<THREE.Texture<HTMLImageElement> | undefined>(undefined);
  public currentHoldColor = computed<THREE.Color>(() =>
    this.resolveHoldColor(Number(this.colorForm.spraywallHoldType().value()) as SpraywallHoldType).clone()
  );
  public resetSignal: Subject<void> = new Subject<void>();
  public undoLastHighlightSignal: Subject<void> = new Subject<void>();
  public spraywallId = '';
  public problemId? = '';
  public spraywallProblemForEdit?: SpraywallProblemDto;
  public holdColorOptions: TypeAndColor[] = holdColorOptions;

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.spraywallId = activatedRoute.snapshot.paramMap.get('spraywallId') ?? '';
    this.problemId = activatedRoute.snapshot.paramMap.get('problemId') ?? undefined;
    this.spraywallProblemForEdit = activatedRoute.snapshot.data['spraywallProblem'];

    // this.subscription.add(
    //   this.resetSignal.subscribe({
    //     next: () => {
    //       this.problemId = undefined;
    //       this.spraywallProblemForEdit = undefined;
    //     }
    //   })
    // );
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
      const routeId = (closeModalEvent.data as { routeId?: string } | undefined)?.routeId;
      this.router.navigate(['/', 'spraywall', this.spraywallId], {
        queryParams: { routeId: routeId ?? null },
        queryParamsHandling: 'merge'
      });
    }
  }

  public onBackToSpraywall(): void {
    const hasUnsavedChanges = this.renderer?.hasUnsavedChanges() ?? false;
    if (hasUnsavedChanges) {
      const shouldDropEditing = window.confirm(
        'Do you really want to drop the current editing? Unsaved changes will be lost.'
      );
      if (!shouldDropEditing) {
        return;
      }
    }

    this.router.navigate(['/', 'spraywall', this.spraywallId]);
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
      dialogData.isWip = this.spraywallProblemForEdit.isWip;
      dialogData.version = this.spraywallProblemForEdit.version;
    }
    component.initialize!(dialogData);
  }

  public enumName(type: SpraywallHoldType): string {
    const enumNames = Object.keys(SpraywallHoldType).filter((key) => isNaN(Number(key)));
    return enumNames[type];
  }

  private resolveHoldColor(type: SpraywallHoldType): THREE.Color {
    const selectedColorOption = this.holdColorOptions.find((option) => option.type === type);
    return selectedColorOption?.color ?? this.holdColorOptions[0].color;
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
    this.currentHighlightUv.set(texture);
  }
}

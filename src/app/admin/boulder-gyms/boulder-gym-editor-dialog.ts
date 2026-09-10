import { Component, computed, effect, inject, output, signal } from '@angular/core';
import { disabled, form, FormField, required } from '@angular/forms/signals';
import { BoulderGymDto, BoulderGymService, CreateBoulderGymCommand, UpdateBoulderGymCommand } from '@api-net/index';
import { ContentFormModel } from '../content-shared/content-form-model';
import { ImageUrlList } from '../content-shared/image-url-list/image-url-list';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { ToastService } from '../../core/toast-container/toast.service';
import { BoulderGymDialogData } from './boulder-gym-dialog-data';

@Component({
  selector: 'app-boulder-gym-editor-dialog',
  imports: [FormField, ImageUrlList],
  templateUrl: './boulder-gym-editor-dialog.html',
  styleUrl: './boulder-gym-editor-dialog.scss'
})
export class BoulderGymEditorDialog implements IModal {
  private boulderGymService = inject(BoulderGymService);
  private toastService = inject(ToastService);

  private isDisabled = signal(false);
  private formModel = signal<ContentFormModel>({ name: '', description: '', importantInfo: '', previewImageUri: '' });
  private editingBoulderGym?: BoulderGymDto;
  public closeModal = output<CloseModalEvent>();
  public disabled = this.isDisabled.asReadonly();

  public canCloseWithoutPermission = true;
  public isLoading = signal(false);
  public images = signal<string[]>([]);
  public title = computed(() => (this.editingBoulderGym ? 'Edit boulder gym' : 'Create boulder gym'));
  public isSubmitDisabled = computed(
    () => this.isLoading() || this.boulderGymForm().disabled() || this.boulderGymForm().invalid()
  );
  public boulderGymForm = form(this.formModel, (schemaPath) => {
    disabled(schemaPath.name, { when: () => this.isDisabled() });
    disabled(schemaPath.description, { when: () => this.isDisabled() });
    disabled(schemaPath.importantInfo, { when: () => this.isDisabled() });
    disabled(schemaPath.previewImageUri, { when: () => this.isDisabled() });
    required(schemaPath.name);
  });

  public constructor() {
    effect(() => {
      this.canCloseWithoutPermission = !this.boulderGymForm().dirty();
    });
  }

  public initialize(data: BoulderGymDialogData): void {
    this.editingBoulderGym = data.boulderGym;
    this.formModel.set({
      name: data.boulderGym?.name ?? '',
      description: data.boulderGym?.description ?? '',
      importantInfo: data.boulderGym?.importantInfo ?? '',
      previewImageUri: data.boulderGym?.previewImageUri ?? ''
    });
    this.images.set(data.boulderGym?.images?.map((image) => image.uri) ?? []);
  }

  public onSave(): void {
    if (this.boulderGymForm().invalid()) {
      return;
    }

    this.isDisabled.set(true);
    this.isLoading.set(true);

    const model = this.formModel();
    const editingBoulderGym = this.editingBoulderGym;

    if (editingBoulderGym) {
      const updateBoulderGym: UpdateBoulderGymCommand = {
        name: model.name,
        description: model.description,
        importantInfo: model.importantInfo,
        previewImageUri: model.previewImageUri,
        imageUris: this.images(),
        version: editingBoulderGym.version
      };
      this.boulderGymService.updateBoulderGym(editingBoulderGym.id!, updateBoulderGym).subscribe({
        next: (boulderGym: BoulderGymDto) => {
          this.isLoading.set(false);
          this.canCloseWithoutPermission = true;
          this.toastService.showSuccess('Boulder gym saved', `“${boulderGym.name}” was updated successfully.`);
          this.closeModal.emit({ closeType: 0, data: boulderGym });
        },
        error: () => {
          this.isDisabled.set(false);
          this.isLoading.set(false);
          this.canCloseWithoutPermission = false;
          this.toastService.showDanger('Unable to save boulder gym', 'Review the fields and try again.');
        }
      });
    } else {
      const createBoulderGym: CreateBoulderGymCommand = {
        name: model.name,
        description: model.description,
        importantInfo: model.importantInfo,
        previewImageUri: model.previewImageUri,
        imageUris: this.images()
      };
      this.boulderGymService.createBoulderGym(createBoulderGym).subscribe({
        next: (boulderGym: BoulderGymDto) => {
          this.isLoading.set(false);
          this.canCloseWithoutPermission = true;
          this.toastService.showSuccess('Boulder gym saved', `“${boulderGym.name}” was created successfully.`);
          this.closeModal.emit({ closeType: 0, data: boulderGym });
        },
        error: () => {
          this.isDisabled.set(false);
          this.isLoading.set(false);
          this.canCloseWithoutPermission = false;
          this.toastService.showDanger('Unable to save boulder gym', 'Review the fields and try again.');
        }
      });
    }
  }
}

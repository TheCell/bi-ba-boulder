import { Component, computed, effect, inject, output, signal } from '@angular/core';
import { disabled, form, FormField, required } from '@angular/forms/signals';
import {
  CreateOutdoorAreaCommand,
  OutdoorAreaDto,
  OutdoorAreasService,
  SectorDto,
  SectorsService,
  UpdateOutdoorAreaCommand
} from '@api-net/index';
import { ContentFormModel } from '../content-shared/content-form-model';
import { ImageUrlList } from '../content-shared/image-url-list/image-url-list';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { ToastService } from '../../core/toast-container/toast.service';
import { OutdoorAreaDialogData } from './outdoor-area-dialog-data';

@Component({
  selector: 'app-outdoor-area-editor-dialog',
  imports: [FormField, ImageUrlList],
  templateUrl: './outdoor-area-editor-dialog.html',
  styleUrl: './outdoor-area-editor-dialog.scss'
})
export class OutdoorAreaEditorDialog implements IModal {
  private outdoorAreasService = inject(OutdoorAreasService);
  private sectorsService = inject(SectorsService);
  private toastService = inject(ToastService);

  private isDisabled = signal(false);
  private formModel = signal<ContentFormModel>({ name: '', description: '', importantInfo: '', previewImageUri: '' });
  private editingOutdoorArea?: OutdoorAreaDto;

  public closeModal = output<CloseModalEvent>();
  public disabled = this.isDisabled.asReadonly();
  public canCloseWithoutPermission = true;
  public isLoading = signal(true);
  public images = signal<string[]>([]);
  public availableSectors = signal<SectorDto[]>([]);
  public selectedSectorIds = signal<Set<string>>(new Set());
  public title = computed(() => (this.editingOutdoorArea ? 'Edit outdoor area' : 'Create outdoor area'));
  public isSubmitDisabled = computed(
    () => this.isLoading() || this.outdoorAreaForm().disabled() || this.outdoorAreaForm().invalid()
  );
  public outdoorAreaForm = form(this.formModel, (schemaPath) => {
    disabled(schemaPath.name, { when: () => this.isDisabled() });
    disabled(schemaPath.description, { when: () => this.isDisabled() });
    disabled(schemaPath.importantInfo, { when: () => this.isDisabled() });
    disabled(schemaPath.previewImageUri, { when: () => this.isDisabled() });
    required(schemaPath.name);
  });

  public constructor() {
    effect(() => {
      this.canCloseWithoutPermission = !this.outdoorAreaForm().dirty();
    });
  }

  public initialize(data: OutdoorAreaDialogData): void {
    this.editingOutdoorArea = data.outdoorArea;
    this.formModel.set({
      name: data.outdoorArea?.name ?? '',
      description: data.outdoorArea?.description ?? '',
      importantInfo: data.outdoorArea?.importantInfo ?? '',
      previewImageUri: data.outdoorArea?.previewImageUri ?? ''
    });
    this.images.set(data.outdoorArea?.images?.map((image) => image.uri) ?? []);
    this.selectedSectorIds.set(new Set(data.outdoorArea?.sectors?.map((sector) => sector.id) ?? []));
    this.loadSectors();
  }

  public onSectorToggle(sectorId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.selectedSectorIds.update((ids) => {
      const next = new Set(ids);
      if (checked) {
        next.add(sectorId);
      } else {
        next.delete(sectorId);
      }
      return next;
    });
  }

  public onSaveAndClose(): void {
    if (this.outdoorAreaForm().invalid()) {
      return;
    }

    this.isDisabled.set(true);
    this.isLoading.set(true);

    const model = this.formModel();
    // TODO: this must be in a separate dialog and made like the boulder gym spraywalls dialog
    const sectorIds = [...this.selectedSectorIds()];
    const editingOutdoorArea = this.editingOutdoorArea;

    if (editingOutdoorArea) {
      const updateOutdoorArea: UpdateOutdoorAreaCommand = {
        name: model.name,
        description: model.description,
        importantInfo: model.importantInfo,
        previewImageUri: model.previewImageUri,
        imageUris: this.images(),
        sectorIds,
        version: editingOutdoorArea.version
      };
      this.outdoorAreasService.updateOutdoorArea(editingOutdoorArea.id!, updateOutdoorArea).subscribe({
        next: (outdoorArea: OutdoorAreaDto) => {
          this.isLoading.set(false);
          this.canCloseWithoutPermission = true;
          this.toastService.showSuccess('Outdoor area saved', `“${outdoorArea.name}” was updated successfully.`);
          this.closeModal.emit({ closeType: 0, data: outdoorArea });
        },
        error: () => {
          this.isDisabled.set(false);
          this.isLoading.set(false);
          this.canCloseWithoutPermission = false;
        }
      });
    } else {
      const createOutdoorArea: CreateOutdoorAreaCommand = {
        name: model.name,
        description: model.description,
        importantInfo: model.importantInfo,
        previewImageUri: model.previewImageUri,
        imageUris: this.images(),
        sectorIds
      };
      this.outdoorAreasService.createOutdoorArea(createOutdoorArea).subscribe({
        next: (outdoorArea: OutdoorAreaDto) => {
          this.isLoading.set(false);
          this.canCloseWithoutPermission = true;
          this.toastService.showSuccess('Outdoor area saved', `"${outdoorArea.name}" was created successfully.`);
          this.closeModal.emit({ closeType: 0, data: outdoorArea });
        },
        error: () => {
          this.isDisabled.set(false);
          this.isLoading.set(false);
          this.canCloseWithoutPermission = false;
        }
      });
    }
  }

  private loadSectors(): void {
    this.isLoading.set(true);
    this.sectorsService.getSectors().subscribe({
      next: (sectors: SectorDto[]) => {
        this.availableSectors.set([...sectors].sort((left, right) => left.name.localeCompare(right.name)));
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showDanger('Unable to load sectors', 'Sectors could not be loaded.');
      }
    });
  }
}

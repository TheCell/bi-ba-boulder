import { Component, computed, effect, inject, output, signal } from '@angular/core';
import { disabled, form, FormField, required } from '@angular/forms/signals';
import {
  CreateSectorCommand,
  OutdoorAreaDto,
  OutdoorAreasService,
  SectorDto,
  SectorsService,
  UpdateSectorCommand
} from '@api-net/index';
import { ContentFormModel } from '../content-shared/content-form-model';
import { ImageUrlList } from '../content-shared/image-url-list/image-url-list';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { ToastService } from '../../core/toast-container/toast.service';
import { SectorDialogData } from './sector-dialog-data';

interface SectorFormModel extends ContentFormModel {
  coordinates: string;
  isPublic: boolean;
}

@Component({
  selector: 'app-sector-editor-dialog',
  imports: [FormField, ImageUrlList],
  templateUrl: './sector-editor-dialog.html',
  styleUrl: './sector-editor-dialog.scss'
})
export class SectorEditorDialog implements IModal {
  private sectorsService = inject(SectorsService);
  private outdoorAreasService = inject(OutdoorAreasService);
  private toastService = inject(ToastService);

  private isDisabled = signal(false);
  private formModel = signal<SectorFormModel>({
    name: '',
    description: '',
    importantInfo: '',
    previewImageUri: '',
    coordinates: '',
    isPublic: false
  });
  private editingSector?: SectorDto;

  public closeModal = output<CloseModalEvent>();
  public disabled = this.isDisabled.asReadonly();
  public canCloseWithoutPermission = true;
  public isLoading = signal(true);
  public images = signal<string[]>([]);
  public availableOutdoorAreas = signal<OutdoorAreaDto[]>([]);
  public selectedOutdoorAreaIds = signal<Set<string>>(new Set());
  public title = computed(() => (this.editingSector ? 'Edit sector' : 'Create sector'));
  public isSubmitDisabled = computed(
    () => this.isLoading() || this.sectorForm().disabled() || this.sectorForm().invalid()
  );
  public sectorForm = form(this.formModel, (schemaPath) => {
    disabled(schemaPath.name, { when: () => this.isDisabled() });
    disabled(schemaPath.description, { when: () => this.isDisabled() });
    disabled(schemaPath.importantInfo, { when: () => this.isDisabled() });
    disabled(schemaPath.previewImageUri, { when: () => this.isDisabled() });
    disabled(schemaPath.coordinates, { when: () => this.isDisabled() });
    disabled(schemaPath.isPublic, { when: () => this.isDisabled() });
    required(schemaPath.name);
  });

  public constructor() {
    effect(() => {
      this.canCloseWithoutPermission = !this.sectorForm().dirty();
    });
  }

  public initialize(data: SectorDialogData): void {
    this.editingSector = data.sector;
    this.formModel.set({
      name: data.sector?.name ?? '',
      description: data.sector?.description ?? '',
      importantInfo: data.sector?.importantInfo ?? '',
      previewImageUri: data.sector?.previewImageUri ?? '',
      coordinates: data.sector?.coordinates ?? '',
      isPublic: data.sector?.isPublic ?? false
    });
    this.images.set(data.sector?.images?.map((image) => image.uri) ?? []);
    this.selectedOutdoorAreaIds.set(new Set(data.sector?.outdoorAreas?.map((outdoorArea) => outdoorArea.id!) ?? []));
    this.loadOutdoorAreas();
  }

  public onOutdoorAreaToggle(outdoorAreaId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.selectedOutdoorAreaIds.update((ids) => {
      const next = new Set(ids);
      if (checked) {
        next.add(outdoorAreaId);
      } else {
        next.delete(outdoorAreaId);
      }
      return next;
    });
  }

  public onSubmit(): void {
    if (this.sectorForm().invalid()) {
      return;
    }

    this.isDisabled.set(true);
    this.isLoading.set(true);

    const model = this.formModel();
    const outdoorAreaIds = [...this.selectedOutdoorAreaIds()];
    const editingSector = this.editingSector;

    if (editingSector) {
      const updateSector: UpdateSectorCommand = {
        name: model.name,
        description: model.description,
        importantInfo: model.importantInfo,
        previewImageUri: model.previewImageUri,
        imageUris: this.images(),
        coordinates: model.coordinates,
        isPublic: model.isPublic,
        outdoorAreaIds,
        version: editingSector.version
      };
      this.sectorsService.updateSector(editingSector.id, updateSector).subscribe({
        next: (sector: SectorDto) => {
          this.isLoading.set(false);
          this.canCloseWithoutPermission = true;
          this.toastService.showSuccess('Sector saved', `“${sector.name}” was updated successfully.`);
          this.closeModal.emit({ closeType: 0, data: sector });
        },
        error: () => {
          this.isDisabled.set(false);
          this.isLoading.set(false);
          this.canCloseWithoutPermission = false;
        }
      });
    } else {
      const createSector: CreateSectorCommand = {
        name: model.name,
        description: model.description,
        importantInfo: model.importantInfo,
        previewImageUri: model.previewImageUri,
        imageUris: this.images(),
        coordinates: model.coordinates,
        isPublic: model.isPublic,
        outdoorAreaIds
      };
      this.sectorsService.createSector(createSector).subscribe({
        next: (sector: SectorDto) => {
          this.isLoading.set(false);
          this.canCloseWithoutPermission = true;
          this.toastService.showSuccess('Sector saved', `“${sector.name}” was created successfully.`);
          this.closeModal.emit({ closeType: 0, data: sector });
        },
        error: () => {
          this.isDisabled.set(false);
          this.isLoading.set(false);
          this.canCloseWithoutPermission = false;
        }
      });
    }
  }

  private loadOutdoorAreas(): void {
    this.isLoading.set(true);
    this.outdoorAreasService.getOutdoorAreas().subscribe({
      next: (outdoorAreas: OutdoorAreaDto[]) => {
        this.availableOutdoorAreas.set([...outdoorAreas].sort((left, right) => left.name.localeCompare(right.name)));
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showDanger('Unable to load outdoor areas', 'Outdoor areas could not be loaded.');
      }
    });
  }
}

import { Component, computed, effect, inject, output, signal } from '@angular/core';
import { disabled, form, FormField, required } from '@angular/forms/signals';
import { BlocDto, BlocsService, SectorDto } from '@api-net/index';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { ToastService } from '../../core/toast-container/toast.service';
import { BlocDialogData } from './bloc-dialog-data';

interface BlocFormModel {
  sectorId: string;
  name: string;
  description: string;
  coordinates: string;
  previewImageUri: string;
  blocLowRes: string;
  blocMedRes: string;
  blocHighRes: string;
}

@Component({
  selector: 'app-bloc-editor-dialog',
  imports: [FormField],
  templateUrl: './bloc-editor-dialog.html',
  styleUrl: './bloc-editor-dialog.scss'
})
export class BlocEditorDialog implements IModal {
  private blocsService = inject(BlocsService);
  private toastService = inject(ToastService);

  private isDisabled = signal(false);
  private formModel = signal<BlocFormModel>({
    sectorId: '',
    name: '',
    description: '',
    coordinates: '',
    previewImageUri: '',
    blocLowRes: '',
    blocMedRes: '',
    blocHighRes: ''
  });
  private editingBloc?: BlocDto;

  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public isLoading = signal(false);
  public sectors = signal<SectorDto[]>([]);
  public title = computed(() => (this.editingBloc ? 'Edit bloc' : 'Create bloc'));
  public isSubmitDisabled = computed(
    () => this.isLoading() || this.blocForm().disabled() || this.blocForm().invalid()
  );
  public blocForm = form(this.formModel, (schemaPath) => {
    disabled(schemaPath.sectorId, { when: () => this.isDisabled() });
    disabled(schemaPath.name, { when: () => this.isDisabled() });
    disabled(schemaPath.description, { when: () => this.isDisabled() });
    disabled(schemaPath.coordinates, { when: () => this.isDisabled() });
    disabled(schemaPath.previewImageUri, { when: () => this.isDisabled() });
    disabled(schemaPath.blocLowRes, { when: () => this.isDisabled() });
    disabled(schemaPath.blocMedRes, { when: () => this.isDisabled() });
    disabled(schemaPath.blocHighRes, { when: () => this.isDisabled() });
    required(schemaPath.sectorId);
    required(schemaPath.name);
  });

  public constructor() {
    effect(() => {
      this.canCloseWithoutPermission = !this.blocForm().dirty();
    });
  }

  public initialize(data: BlocDialogData): void {
    this.editingBloc = data.bloc;
    this.sectors.set([...(data.sectors ?? [])].sort((left, right) => left.name.localeCompare(right.name)));
    this.formModel.set({
      sectorId: data.bloc?.sectorId ?? data.sectorId ?? '',
      name: data.bloc?.name ?? '',
      description: data.bloc?.description ?? '',
      coordinates: data.bloc?.coordinates ?? '',
      previewImageUri: data.bloc?.previewImageUri ?? '',
      blocLowRes: data.bloc?.blocLowRes ?? '',
      blocMedRes: data.bloc?.blocMedRes ?? '',
      blocHighRes: data.bloc?.blocHighRes ?? ''
    });
  }

  public onSubmit(): void {
    if (this.blocForm().invalid()) {
      return;
    }

    this.isDisabled.set(true);
    this.isLoading.set(true);

    const model = this.formModel();
    const editingBloc = this.editingBloc;

    const request$ = editingBloc
      ? this.blocsService.updateBloc(editingBloc.id, { ...model, version: editingBloc.version })
      : this.blocsService.createBloc(model);

    request$.subscribe({
      next: (bloc: BlocDto) => {
        this.isLoading.set(false);
        this.canCloseWithoutPermission = true;
        this.toastService.showSuccess(
          'Bloc saved',
          `“${bloc.name}” was ${editingBloc ? 'updated' : 'created'} successfully.`
        );
        this.closeModal.emit({ closeType: 0, data: bloc });
      },
      error: () => {
        this.isDisabled.set(false);
        this.isLoading.set(false);
        this.canCloseWithoutPermission = false;
      }
    });
  }
}

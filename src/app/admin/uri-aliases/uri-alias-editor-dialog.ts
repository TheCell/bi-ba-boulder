import { Component, computed, effect, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { disabled, form, FormField, minLength, required } from '@angular/forms/signals';
import {
  BoulderGymDto,
  BoulderGymService,
  CreateUriAliasCommand,
  MediasService,
  OutdoorAreaDto,
  OutdoorAreasService,
  UpdateUriAliasCommand,
  UriAliasAdministrationDto
} from '@api-net/index';
import { forkJoin } from 'rxjs';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { ToastService } from '../../core/toast-container/toast.service';
import { UriAliasDialogData } from './uri-alias-dialog-data';

interface UriAliasFormModel {
  alias: string;
  typeId: string;
  targetId: string;
}

interface UriAliasTarget {
  id: string;
  name: string;
}

@Component({
  selector: 'app-uri-alias-editor-dialog',
  imports: [FormsModule, FormField],
  templateUrl: './uri-alias-editor-dialog.html',
  styleUrl: './uri-alias-editor-dialog.scss'
})
export class UriAliasEditorDialog implements IModal {
  private mediasService = inject(MediasService);
  private boulderGymService = inject(BoulderGymService);
  private outdoorAreasService = inject(OutdoorAreasService);
  private toastService = inject(ToastService);

  private isDisabled = signal(false);
  private formModel = signal<UriAliasFormModel>({ alias: '', typeId: '0', targetId: '' });
  private boulderGyms = signal<UriAliasTarget[]>([]);
  private outdoorAreas = signal<UriAliasTarget[]>([]);
  private editingAlias?: UriAliasAdministrationDto;

  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public isLoading = signal(true);
  public targetSearch = signal('');
  public title = computed(() => (this.editingAlias ? 'Edit URI alias' : 'Create URI alias'));
  public availableTargets = computed(() => {
    const targets = this.formModel().typeId === '1' ? this.boulderGyms() : this.outdoorAreas();
    const search = this.targetSearch().trim().toLocaleLowerCase();
    return search.length === 0 ? targets : targets.filter((target) => target.name.toLocaleLowerCase().includes(search));
  });
  public isSubmitDisabled = computed(
    () => this.isLoading() || this.uriAliasForm().disabled() || this.uriAliasForm().invalid()
  );
  public uriAliasForm = form(this.formModel, (schemaPath) => {
    disabled(schemaPath.alias, { when: () => this.isDisabled() });
    disabled(schemaPath.typeId, { when: () => this.isDisabled() });
    disabled(schemaPath.targetId, { when: () => this.isDisabled() });
    required(schemaPath.alias);
    minLength(schemaPath.alias, 1);
    required(schemaPath.typeId);
    required(schemaPath.targetId);
  });

  public constructor() {
    effect(() => {
      this.canCloseWithoutPermission = !this.uriAliasForm().dirty();
    });
  }

  public initialize(data: UriAliasDialogData): void {
    this.editingAlias = data.uriAlias;
    this.formModel.set({
      alias: data.uriAlias?.alias ?? '',
      typeId: data.uriAlias?.typeId.toString() ?? '0',
      targetId: data.uriAlias?.targetId ?? ''
    });
    this.loadTargets();
  }

  public onTypeChanged(): void {
    this.targetSearch.set('');
    this.formModel.update((model) => ({ ...model, targetId: '' }));
  }

  public onTargetSearch(event: Event): void {
    this.targetSearch.set((event.target as HTMLInputElement).value);
  }

  public onSubmit(): void {
    if (this.uriAliasForm().invalid()) {
      return;
    }

    this.isDisabled.set(true);
    this.isLoading.set(true);
    const model = this.formModel();
    const editingAlias = this.editingAlias;
    const request = editingAlias
      ? this.mediasService.updateUriAlias(editingAlias.id, {
          alias: model.alias,
          typeId: Number(model.typeId),
          targetId: model.targetId,
          version: editingAlias.version
        } satisfies UpdateUriAliasCommand)
      : this.mediasService.createUriAlias({
          alias: model.alias,
          typeId: Number(model.typeId),
          targetId: model.targetId
        } satisfies CreateUriAliasCommand);

    request.subscribe({
      next: (uriAlias: UriAliasAdministrationDto) => {
        this.isLoading.set(false);
        this.canCloseWithoutPermission = true;
        this.toastService.showSuccess('URI alias saved', `The alias “${uriAlias.alias}” was saved.`);
        this.closeModal.emit({ closeType: 0, data: uriAlias });
      },
      error: () => {
        this.isDisabled.set(false);
        this.isLoading.set(false);
        this.canCloseWithoutPermission = false;
        this.toastService.showDanger('Unable to save URI alias', 'Review the alias and target, then try again.');
      }
    });
  }

  private loadTargets(): void {
    forkJoin({
      boulderGyms: this.boulderGymService.getBoulderGyms(),
      outdoorAreas: this.outdoorAreasService.getOutdoorAreas()
    }).subscribe({
      next: ({ boulderGyms, outdoorAreas }: { boulderGyms: BoulderGymDto[]; outdoorAreas: OutdoorAreaDto[] }) => {
        this.boulderGyms.set(this.toTargets(boulderGyms));
        this.outdoorAreas.set(this.toTargets(outdoorAreas));
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showDanger('Unable to load targets', 'Boulder gyms and outdoor areas could not be loaded.');
      }
    });
  }

  private toTargets(items: (BoulderGymDto | OutdoorAreaDto)[]): UriAliasTarget[] {
    return items
      .filter((item): item is (BoulderGymDto | OutdoorAreaDto) & { id: string } => item.id !== undefined)
      .map((item) => ({ id: item.id, name: item.name }))
      .sort((left, right) => left.name.localeCompare(right.name));
  }
}

import { Component, computed, inject, OnInit, signal, ViewChild } from '@angular/core';
import { MediasService, UriAliasAdministrationDto } from '@api-net/index';
import { Modal } from '../../core/modal/modal/modal';
import { ModalService } from '../../core/modal/modal.service';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ToastService } from '../../core/toast-container/toast.service';
import { DeleteUriAliasDialog } from './delete-uri-alias-dialog';
import { UriAliasEditorDialog } from './uri-alias-editor-dialog';
import { UriType } from '../../core/enums/uri-type.enum';

@Component({
  selector: 'app-uri-aliases',
  imports: [Modal],
  templateUrl: './uri-aliases.html',
  styleUrl: './uri-aliases.scss'
})
export class UriAliases implements OnInit {
  @ViewChild('editorModal') private editorModal!: Modal;
  @ViewChild('deleteModal') private deleteModal!: Modal;

  private mediasService = inject(MediasService);
  private modalService = inject(ModalService);
  private toastService = inject(ToastService);

  public uriAliases = signal<UriAliasAdministrationDto[]>([]);
  public search = signal('');
  public typeFilter = signal('all');
  public isLoading = signal(true);
  public filteredUriAliases = computed(() => {
    const search = this.search().trim().toLocaleLowerCase();
    const typeFilter = this.typeFilter();
    return this.uriAliases().filter(
      (uriAlias) =>
        (typeFilter === 'all' || uriAlias.typeId.toString() === typeFilter) &&
        (search.length === 0 ||
          uriAlias.alias.toLocaleLowerCase().includes(search) ||
          uriAlias.targetName.toLocaleLowerCase().includes(search))
    );
  });

  public ngOnInit(): void {
    this.loadUriAliases();
  }

  public onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  public onTypeFilter(event: Event): void {
    this.typeFilter.set((event.target as HTMLSelectElement).value);
  }

  public openCreateDialog(): void {
    const dialog = this.modalService.open(this.editorModal.id, UriAliasEditorDialog) as UriAliasEditorDialog;
    dialog.initialize({});
  }

  public openEditDialog(uriAlias: UriAliasAdministrationDto): void {
    const dialog = this.modalService.open(this.editorModal.id, UriAliasEditorDialog) as UriAliasEditorDialog;
    dialog.initialize({ uriAlias });
  }

  public openDeleteDialog(uriAlias: UriAliasAdministrationDto): void {
    const dialog = this.modalService.open(this.deleteModal.id, DeleteUriAliasDialog) as DeleteUriAliasDialog;
    dialog.initialize({ uriAlias });
  }

  public onEditorClosed(event: CloseModalEvent): void {
    if (event.closeType === 0) {
      this.loadUriAliases();
    }
  }

  public onDeleteClosed(event: CloseModalEvent): void {
    if (event.closeType !== 0) {
      return;
    }

    const uriAlias = event.data as UriAliasAdministrationDto;
    this.mediasService.deleteUriAlias(uriAlias.id, { version: uriAlias.version }).subscribe({
      next: () => {
        this.uriAliases.update((items) => items.filter((item) => item.id !== uriAlias.id));
        this.toastService.showSuccess('URI alias deleted', `The alias “${uriAlias.alias}” was deleted.`);
      },
      error: () => this.toastService.showDanger('Unable to delete URI alias', 'Reload the page and try again.')
    });
  }

  public getTypeName(typeId: number): string {
    return typeId === UriType.BoulderGym ? 'Boulder gym' : 'Outdoor area';
  }

  private loadUriAliases(): void {
    this.isLoading.set(true);
    this.mediasService.getUriAliases().subscribe({
      next: (uriAliases: UriAliasAdministrationDto[]) => {
        this.uriAliases.set(uriAliases);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showDanger('Unable to load URI aliases', 'Try refreshing the page.');
      }
    });
  }
}

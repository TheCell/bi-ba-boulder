import { Component, computed, inject, OnInit, signal, viewChild } from '@angular/core';
import { OutdoorAreaDto, OutdoorAreasService } from '@api-net/index';
import { Modal } from '../../core/modal/modal/modal';
import { ModalService } from '../../core/modal/modal.service';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ToastService } from '../../core/toast-container/toast.service';
import { OutdoorAreaEditorDialog } from './outdoor-area-editor-dialog';
import { DeleteOutdoorAreaDialog } from './delete-outdoor-area-dialog';

@Component({
  selector: 'app-outdoor-areas-admin',
  imports: [Modal],
  templateUrl: './outdoor-areas-admin.html',
  styleUrl: './outdoor-areas-admin.scss'
})
export class OutdoorAreasAdmin implements OnInit {
  private editorModal = viewChild.required<Modal>('editorModal');
  private deleteModal = viewChild.required<Modal>('deleteModal');

  private outdoorAreasService = inject(OutdoorAreasService);
  private toastService = inject(ToastService);
  private modalService = inject(ModalService);

  public outdoorAreas = signal<OutdoorAreaDto[]>([]);
  public search = signal('');
  public isLoading = signal(true);
  public filteredOutdoorAreas = computed(() => {
    const search = this.search().trim().toLocaleLowerCase();
    return search.length === 0
      ? this.outdoorAreas()
      : this.outdoorAreas().filter((outdoorArea) => outdoorArea.name.toLocaleLowerCase().includes(search));
  });

  public ngOnInit(): void {
    this.loadOutdoorAreas();
  }

  public onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  public openCreateDialog(): void {
    const dialog = this.modalService.open(this.editorModal().id, OutdoorAreaEditorDialog);
    if (dialog !== undefined && typeof dialog.initialize === 'function') {
      dialog.initialize({});
    }
  }

  public openEditDialog(outdoorArea: OutdoorAreaDto): void {
    const dialog = this.modalService.open(this.editorModal().id, OutdoorAreaEditorDialog);
    if (dialog !== undefined && typeof dialog.initialize === 'function') {
      dialog.initialize({ outdoorArea });
    }
  }

  public openDeleteDialog(outdoorArea: OutdoorAreaDto): void {
    const dialog = this.modalService.open(this.deleteModal().id, DeleteOutdoorAreaDialog);
    if (dialog !== undefined && typeof dialog.initialize === 'function') {
      dialog.initialize({ outdoorArea });
    }
  }

  public onEditorClosed(event: CloseModalEvent): void {
    if (event.closeType === 0) {
      this.loadOutdoorAreas();
    }
  }

  public onDeleteClosed(event: CloseModalEvent): void {
    if (event.closeType === 0) {
      const outdoorArea = event.data as OutdoorAreaDto;
      this.outdoorAreas.update((items) => items.filter((item) => item.id !== outdoorArea.id));
    }
  }

  private loadOutdoorAreas(): void {
    this.isLoading.set(true);
    this.outdoorAreasService.getOutdoorAreas().subscribe({
      next: (outdoorAreas: OutdoorAreaDto[]) => {
        this.outdoorAreas.set(outdoorAreas);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showDanger('Unable to load outdoor areas', 'Try refreshing the page.');
      }
    });
  }
}

import { Component, computed, inject, OnInit, signal, ViewChild } from '@angular/core';
import { SectorDto, SectorsService } from '@api-net/index';
import { Modal } from '../../core/modal/modal/modal';
import { ModalService } from '../../core/modal/modal.service';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ToastService } from '../../core/toast-container/toast.service';
import { SectorEditorDialog } from './sector-editor-dialog';
import { DeleteSectorDialog } from './delete-sector-dialog';

@Component({
  selector: 'app-sectors-admin',
  imports: [Modal],
  templateUrl: './sectors-admin.html',
  styleUrl: './sectors-admin.scss'
})
export class SectorsAdmin implements OnInit {
  @ViewChild('editorModal') private editorModal!: Modal;
  @ViewChild('deleteModal') private deleteModal!: Modal;

  private sectorsService = inject(SectorsService);
  private toastService = inject(ToastService);
  private modalService = inject(ModalService);

  public sectors = signal<SectorDto[]>([]);
  public search = signal('');
  public isLoading = signal(true);
  public filteredSectors = computed(() => {
    const search = this.search().trim().toLocaleLowerCase();
    return search.length === 0
      ? this.sectors()
      : this.sectors().filter((sector) => sector.name.toLocaleLowerCase().includes(search));
  });

  public ngOnInit(): void {
    this.loadSectors();
  }

  public onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  public openCreateDialog(): void {
    const dialog = this.modalService.open(this.editorModal.id, SectorEditorDialog);
    if (dialog !== undefined && typeof dialog.initialize === 'function') {
      dialog.initialize({});
    }
  }

  public openEditDialog(sector: SectorDto): void {
    const dialog = this.modalService.open(this.editorModal.id, SectorEditorDialog);
    if (dialog !== undefined && typeof dialog.initialize === 'function') {
      dialog.initialize({ sector });
    }
  }

  public openDeleteDialog(sector: SectorDto): void {
    const dialog = this.modalService.open(this.deleteModal.id, DeleteSectorDialog);
    if (dialog !== undefined && typeof dialog.initialize === 'function') {
      dialog.initialize({ sector });
    }
  }

  public onEditorClosed(event: CloseModalEvent): void {
    if (event.closeType === 0) {
      this.loadSectors();
    }
  }

  public onDeleteClosed(event: CloseModalEvent): void {
    if (event.closeType !== 0) {
      return;
    }

    const sector = event.data as SectorDto;
    this.sectors.update((items) => items.filter((item) => item.id !== sector.id));
  }

  private loadSectors(): void {
    this.isLoading.set(true);
    this.sectorsService.getSectors().subscribe({
      next: (sectors: SectorDto[]) => {
        this.sectors.set(sectors);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showDanger('Unable to load sectors', 'Try refreshing the page.');
      }
    });
  }
}

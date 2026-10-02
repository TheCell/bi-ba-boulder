import { Component, computed, inject, OnInit, signal, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BlocDto, BlocsService, SectorDto, SectorsService } from '@api-net/index';
import { Modal } from '../../core/modal/modal/modal';
import { ModalService } from '../../core/modal/modal.service';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ToastService } from '../../core/toast-container/toast.service';
import { BlocEditorDialog } from './bloc-editor-dialog';
import { DeleteBlocDialog } from './delete-bloc-dialog';

@Component({
  selector: 'app-blocs-admin',
  imports: [Modal],
  templateUrl: './blocs-admin.html',
  styleUrl: './blocs-admin.scss'
})
export class BlocsAdmin implements OnInit {
  @ViewChild('editorModal') private editorModal!: Modal;
  @ViewChild('deleteModal') private deleteModal!: Modal;

  private blocsService = inject(BlocsService);
  private sectorsService = inject(SectorsService);
  private toastService = inject(ToastService);
  private modalService = inject(ModalService);
  private activatedRoute = inject(ActivatedRoute);
  private router = inject(Router);

  public sectors = signal<SectorDto[]>([]);
  public selectedSectorId = signal<string | null>(null);
  public blocs = signal<BlocDto[]>([]);
  public search = signal('');
  public isLoading = signal(true);
  public filteredBlocs = computed(() => {
    const search = this.search().trim().toLocaleLowerCase();
    return search.length === 0
      ? this.blocs()
      : this.blocs().filter((bloc) => bloc.name.toLocaleLowerCase().includes(search));
  });

  public ngOnInit(): void {
    this.loadSectors();
  }

  public onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  public onSectorChange(event: Event): void {
    this.selectSector((event.target as HTMLSelectElement).value || null);
  }

  public openCreateDialog(): void {
    const dialog = this.modalService.open(this.editorModal.id, BlocEditorDialog);
    if (dialog !== undefined && typeof dialog.initialize === 'function') {
      dialog.initialize({ sectors: this.sectors(), sectorId: this.selectedSectorId() ?? undefined });
    }
  }

  public openEditDialog(bloc: BlocDto): void {
    const dialog = this.modalService.open(this.editorModal.id, BlocEditorDialog);
    if (dialog !== undefined && typeof dialog.initialize === 'function') {
      dialog.initialize({ bloc, sectors: this.sectors() });
    }
  }

  public openDeleteDialog(bloc: BlocDto): void {
    const dialog = this.modalService.open(this.deleteModal.id, DeleteBlocDialog);
    if (dialog !== undefined && typeof dialog.initialize === 'function') {
      dialog.initialize({ bloc });
    }
  }

  public onEditorClosed(event: CloseModalEvent): void {
    if (event.closeType === 0) {
      this.loadBlocs();
    }
  }

  public onDeleteClosed(event: CloseModalEvent): void {
    if (event.closeType !== 0) {
      return;
    }

    const bloc = event.data as BlocDto;
    this.blocs.update((items) => items.filter((item) => item.id !== bloc.id));
  }

  private selectSector(sectorId: string | null): void {
    this.selectedSectorId.set(sectorId);
    this.router.navigate([], {
      relativeTo: this.activatedRoute,
      queryParams: { sectorId: sectorId ?? 'unassigned' },
      replaceUrl: true
    });
    this.loadBlocs();
  }

  private loadSectors(): void {
    this.isLoading.set(true);
    this.sectorsService.getSectors().subscribe({
      next: (sectors: SectorDto[]) => {
        const sortedSectors = [...sectors].sort((left, right) => left.name.localeCompare(right.name));
        this.sectors.set(sortedSectors);

        const requestedSectorId = this.activatedRoute.snapshot.queryParamMap.get('sectorId');
        if (requestedSectorId === 'unassigned') {
          this.selectSector(null);
          return;
        }
        const initialSector = sortedSectors.find((sector) => sector.id === requestedSectorId) ?? sortedSectors[0];
        this.selectSector(initialSector?.id ?? null);
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showDanger('Unable to load sectors', 'Try refreshing the page.');
      }
    });
  }

  private loadBlocs(): void {
    const sectorId = this.selectedSectorId();
    this.isLoading.set(true);
    const request$ =
      sectorId === null ? this.blocsService.getBlocsWithoutSector() : this.blocsService.getBlocsBySectorId(sectorId);
    request$.subscribe({
      next: (blocs: BlocDto[]) => {
        this.blocs.set(blocs);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.toastService.showDanger('Unable to load blocs', 'Try refreshing the page.');
      }
    });
  }
}

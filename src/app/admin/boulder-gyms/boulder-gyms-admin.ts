import { Component, computed, inject, OnInit, signal, ViewChild } from '@angular/core';
import { BoulderGymDto, BoulderGymService, DeleteBoulderGymCommand } from '@api-net/index';
import { Modal } from '../../core/modal/modal/modal';
import { ModalService } from '../../core/modal/modal.service';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ToastService } from '../../core/toast-container/toast.service';
import { BoulderGymEditorDialog } from './boulder-gym-editor-dialog';
import { DeleteBoulderGymDialog } from './delete-boulder-gym-dialog';
import { Icon } from '../../core/icon/icon';
import { Router } from '@angular/router';

@Component({
  selector: 'app-boulder-gyms-admin',
  imports: [Modal, Icon],
  templateUrl: './boulder-gyms-admin.html',
  styleUrl: './boulder-gyms-admin.scss'
})
export class BoulderGymsAdmin implements OnInit {
  @ViewChild('editorModal') private editorModal!: Modal;
  @ViewChild('deleteModal') private deleteModal!: Modal;

  private boulderGymService = inject(BoulderGymService);
  private toastService = inject(ToastService);
  private modalService = inject(ModalService);
  private router = inject(Router);

  public boulderGyms = signal<BoulderGymDto[]>([]);
  public search = signal('');
  public isLoading = signal(true);
  public filteredBoulderGyms = computed(() => {
    const search = this.search().trim().toLocaleLowerCase();
    return search.length === 0
      ? this.boulderGyms()
      : this.boulderGyms().filter((boulderGym) => boulderGym.name.toLocaleLowerCase().includes(search));
  });

  public ngOnInit(): void {
    this.loadBoulderGyms();
  }

  public onGoToBoulderGym(boulderGym: BoulderGymDto): void {
    this.router.navigate(['boulder-gym', boulderGym.id]);
  }

  public onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  public openCreateDialog(): void {
    const dialog = this.modalService.open(this.editorModal.id, BoulderGymEditorDialog) as BoulderGymEditorDialog;
    dialog.initialize({});
  }

  public openEditDialog(boulderGym: BoulderGymDto): void {
    const dialog = this.modalService.open(this.editorModal.id, BoulderGymEditorDialog) as BoulderGymEditorDialog;
    dialog.initialize({ boulderGym });
  }

  public openDeleteDialog(boulderGym: BoulderGymDto): void {
    const dialog = this.modalService.open(this.deleteModal.id, DeleteBoulderGymDialog) as DeleteBoulderGymDialog;
    dialog.initialize({ boulderGym });
  }

  public onEditorClosed(event: CloseModalEvent): void {
    if (event.closeType === 0) {
      this.loadBoulderGyms();
    }
  }

  public onDeleteClosed(event: CloseModalEvent): void {
    if (event.closeType !== 0) {
      return;
    }

    const boulderGym = event.data as BoulderGymDto;
    const deleteBoulderGym: DeleteBoulderGymCommand = { version: boulderGym.version };
    console.log(deleteBoulderGym);

    this.boulderGymService.deleteBoulderGym(boulderGym.id!, deleteBoulderGym).subscribe({
      next: () => {
        this.boulderGyms.update((items) => items.filter((item) => item.id !== boulderGym.id));
        this.toastService.showSuccess('Boulder gym deleted', `“${boulderGym.name}” was deleted.`);
      }
    });
  }

  private loadBoulderGyms(): void {
    this.isLoading.set(true);
    this.boulderGymService.getBoulderGyms().subscribe({
      next: (boulderGyms: BoulderGymDto[]) => {
        this.boulderGyms.set(boulderGyms);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }
}

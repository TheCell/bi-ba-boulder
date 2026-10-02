import { Component, inject, output } from '@angular/core';
import { BlocDto, BlocsService, DeleteBlocCommand } from '@api-net/index';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { ToastService } from '../../core/toast-container/toast.service';
import { BlocDialogData } from './bloc-dialog-data';

@Component({
  selector: 'app-delete-bloc-dialog',
  templateUrl: './delete-bloc-dialog.html'
})
export class DeleteBlocDialog implements IModal {
  private blocsService = inject(BlocsService);
  private toastService = inject(ToastService);

  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public bloc: BlocDto = null!;

  public initialize(data: BlocDialogData): void {
    this.bloc = data.bloc!;
  }

  public onDelete(): void {
    const deleteBlocCommand: DeleteBlocCommand = { version: this.bloc.version };
    this.blocsService.deleteBloc(this.bloc.id, deleteBlocCommand).subscribe({
      next: () => {
        this.toastService.showSuccess('Bloc deleted', `"${this.bloc.name}" was deleted.`);
        this.closeModal.emit({ closeType: 0, data: this.bloc });
      },
      error: () =>
        this.toastService.showDanger(
          'Unable to delete bloc',
          `"${this.bloc.name}" could not be deleted. Reload the page and try again.`
        )
    });
  }
}

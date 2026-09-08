import { Component, inject, output } from '@angular/core';
import { DeleteOutdoorAreaCommand, OutdoorAreaDto, OutdoorAreasService } from '@api-net/index';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { OutdoorAreaDialogData } from './outdoor-area-dialog-data';
import { ToastService } from '../../core/toast-container/toast.service';

@Component({
  selector: 'app-delete-outdoor-area-dialog',
  templateUrl: './delete-outdoor-area-dialog.html'
})
export class DeleteOutdoorAreaDialog implements IModal {
  private outdoorAreasService = inject(OutdoorAreasService);
  private toastService = inject(ToastService);

  public closeModal = output<CloseModalEvent>();

  public canCloseWithoutPermission = true;
  public outdoorArea: OutdoorAreaDto = null!;

  public initialize(data: OutdoorAreaDialogData): void {
    this.outdoorArea = data.outdoorArea!;
  }

  public onDelete(): void {
    const deleteOutdoorAreaCommand: DeleteOutdoorAreaCommand = { version: this.outdoorArea.version };
    this.outdoorAreasService.deleteOutdoorArea(this.outdoorArea.id!, deleteOutdoorAreaCommand).subscribe({
      next: () => {
        this.toastService.showSuccess('Outdoor area deleted', `"${this.outdoorArea.name}" was deleted.`);
        this.closeModal.emit({ closeType: 0, data: this.outdoorArea });
      },
      error: () => this.toastService.showDanger('Unable to delete outdoor area', 'Reload the page and try again.')
    });
  }
}

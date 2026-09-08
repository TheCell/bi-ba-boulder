import { Component, inject, output } from '@angular/core';
import { DeleteSectorCommand, SectorDto, SectorsService } from '@api-net/index';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { SectorDialogData } from './sector-dialog-data';
import { ToastService } from '../../core/toast-container/toast.service';

@Component({
  selector: 'app-delete-sector-dialog',
  templateUrl: './delete-sector-dialog.html'
})
export class DeleteSectorDialog implements IModal {
  private sectorsService = inject(SectorsService);
  private toastService = inject(ToastService);

  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public sector: SectorDto = null!;

  public initialize(data: SectorDialogData): void {
    this.sector = data.sector!;
  }

  public onDelete(): void {
    const deleteSectorCommand: DeleteSectorCommand = { version: this.sector.version };
    this.sectorsService.deleteSector(this.sector.id, deleteSectorCommand).subscribe({
      next: () => {
        this.toastService.showSuccess('Sector deleted', `"${this.sector.name}" was deleted.`);
        this.closeModal.emit({ closeType: 0, data: this.sector });
      },
      error: () =>
        this.toastService.showDanger(
          'Unable to delete sector',
          `"${this.sector.name}" could not be deleted. Reload the page and try again.`
        )
    });
  }
}

import { Component, output } from '@angular/core';
import { ConfirmDeleteDialogData } from './confirm-delete-dialog-data';
import { IModal } from '../../core/modal/modal/modal.interface';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { LineDto } from '@api-net/model/models';
import { FontGradePipePipe } from '../../core/pipes/font-grade-pipe-pipe';

@Component({
  selector: 'app-confirm-delete-dialog',
  imports: [FontGradePipePipe],
  templateUrl: './confirm-delete-dialog.html',
  styleUrl: './confirm-delete-dialog.scss'
})
export class ConfirmDeleteDialog implements IModal {
  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public lineDto: LineDto = null!;

  public initialize(data: ConfirmDeleteDialogData): void {
    this.lineDto = data.line;
  }
}


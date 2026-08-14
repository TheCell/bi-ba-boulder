import { Component, output } from '@angular/core';
import { IModal } from '../../core/modal/modal/modal.interface';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { LineDto } from '@api-net/model/models';
import { FontGradePipePipe } from '../../core/pipes/font-grade-pipe-pipe';
import { ConfirmDeleteOutdoorsDialogData } from './confirm-delete-outdoors-dialog-data';

@Component({
  selector: 'app-confirm-delete-outdoors-dialog',
  imports: [FontGradePipePipe],
  templateUrl: './confirm-delete-outdoors-dialog.html',
  styleUrl: './confirm-delete-outdoors-dialog.scss'
})
export class ConfirmDeleteOutdoorsDialog implements IModal {
  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public lineDto: LineDto = null!;

  public initialize(data: ConfirmDeleteOutdoorsDialogData): void {
    this.lineDto = data.line;
  }
}


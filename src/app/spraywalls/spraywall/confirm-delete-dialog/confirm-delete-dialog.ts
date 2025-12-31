import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, output } from '@angular/core';
import { CloseModalEvent } from 'src/app/core/modal/modal/close-modal-event';
import { IModal } from 'src/app/core/modal/modal/modal.interface';
import { ConfirmDeleteDialogData } from './confirm-delete-dialog-data';
import { SpraywallProblemDto } from '@api/index';
import { FontGradePipePipe } from 'src/app/core/pipes/font-grade-pipe-pipe';

@Component({
  selector: 'app-confirm-delete-dialog',
  imports: [CommonModule, FontGradePipePipe],
  templateUrl: './confirm-delete-dialog.html',
  styleUrl: './confirm-delete-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDeleteDialog implements IModal {
  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public spraywallProblemDto: SpraywallProblemDto = null!;

  public initialize(data: ConfirmDeleteDialogData): void {
    this.spraywallProblemDto = data.spraywallProblem;
  }
}

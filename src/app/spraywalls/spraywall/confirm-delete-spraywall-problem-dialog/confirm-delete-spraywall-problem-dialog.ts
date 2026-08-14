import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, output } from '@angular/core';
import { ConfirmDeleteSpraywallProblemDialogData } from './confirm-delete-spraywall-problem-dialog-data';
import { SpraywallProblemDto } from '@api-net/index';
import { FontGradePipePipe } from '../../../core/pipes/font-grade-pipe-pipe';
import { IModal } from '../../../core/modal/modal/modal.interface';
import { CloseModalEvent } from '../../../core/modal/modal/close-modal-event';

@Component({
  selector: 'app-confirm-delete-dialog',
  imports: [CommonModule, FontGradePipePipe],
  templateUrl: './confirm-delete-spraywall-problem-dialog.html',
  styleUrl: './confirm-delete-spraywall-problem-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmDeleteDialog implements IModal {
  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public spraywallProblemDto: SpraywallProblemDto = null!;

  public initialize(data: ConfirmDeleteSpraywallProblemDialogData): void {
    this.spraywallProblemDto = data.spraywallProblem;
  }
}

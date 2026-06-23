import { ChangeDetectionStrategy, Component, effect, inject, input, signal, ViewChild } from '@angular/core';
import { BoulderLogDto, BoulderLogsService, CreateBoulderLogCommand, UpdateBoulderLogCommand } from '@api-net/index';
import { ProblemLogDialog } from './problem-log-dialog/problem-log-dialog';
import { ProblemLogDialogData } from './problem-log-dialog/problem-log-dialog-data.interface';
import { Icon } from '../../../core/icon/icon';
import { Modal } from '../../../core/modal/modal/modal';
import { ModalService } from '../../../core/modal/modal.service';
import { CloseModalEvent } from '../../../core/modal/modal/close-modal-event';
import { ToastService } from '../../../core/toast-container/toast.service';

@Component({
  selector: 'app-problem-log-overlay',
  imports: [Icon, Modal],
  templateUrl: './problem-log-overlay.html',
  styleUrl: './problem-log-overlay.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProblemLogOverlay {
  @ViewChild('modal') private modal!: Modal;

  private boulderLogsService = inject(BoulderLogsService);
  private modalService = inject(ModalService);
  private toastService = inject(ToastService);

  public spraywallProblemId = input.required<string>();
  public boulderLog = input<BoulderLogDto>();

  public currentBoulderLog = signal<BoulderLogDto | undefined>(undefined);
  public isLoadingIsProject = signal<boolean>(false);
  public isLoadingIsSent = signal<boolean>(false);
  public isLoadingModal = signal<boolean>(false);

  public constructor() {
    effect(() => {
      this.currentBoulderLog.set(this.boulderLog());
    });
  }

  public onCreateClick(action: 'sent' | 'project'): void {
    const newLog: CreateBoulderLogCommand = {
      isProject: action === 'project',
      isSent: action === 'sent'
    };
    this.boulderLogsService.createBoulderLogForSpraywall(this.spraywallProblemId(), newLog).subscribe({
      next: (boulderLog?: BoulderLogDto) => {
        this.currentBoulderLog.set(boulderLog);
      }
    });
  }

  public onUpdateClick(action: 'sent' | 'project'): void {
    if (this.isLoadingIsSent() || this.isLoadingIsProject()) {
      return;
    }

    if (action === 'sent') {
      this.isLoadingIsSent.set(true);
    } else {
      this.isLoadingIsProject.set(true);
    }
    const updateLog: UpdateBoulderLogCommand = {
      isProject: this.currentBoulderLog()!.isProject,
      isSent: this.currentBoulderLog()!.isSent,
      id: this.currentBoulderLog()!.id,
      version: this.currentBoulderLog()!.version
    };

    if (action === 'project') {
      updateLog.isProject = !updateLog.isProject;
    } else {
      updateLog.isSent = !updateLog.isSent;
    }

    this.boulderLogsService.updateBoulderLog(this.currentBoulderLog()!.id, updateLog).subscribe({
      next: (boulderLog?: BoulderLogDto) => {
        this.currentBoulderLog.set(boulderLog);
        if (action === 'sent') {
          this.isLoadingIsSent.set(false);
        } else {
          this.isLoadingIsProject.set(false);
        }
      },
      error: () => {
        if (action === 'sent') {
          this.isLoadingIsSent.set(false);
        } else {
          this.isLoadingIsProject.set(false);
        }
      }
    });
  }

  public onExpandClick(): void {
    if (this.isLoadingModal()) {
      return;
    }
    this.isLoadingModal.set(true);

    const problemLogDialogData: ProblemLogDialogData = {
      spraywallProblemId: this.spraywallProblemId(),
      problemLog: this.currentBoulderLog()
    };
    const modal = this.modalService.open(this.modal.id, ProblemLogDialog);
    if (modal && modal.initialize) {
      modal.initialize(problemLogDialogData);
    }
  }

  public closeModal(closeModalEvent: CloseModalEvent) {
    if (closeModalEvent.closeType === 0) {
      const boulderLog = closeModalEvent.data as BoulderLogDto | undefined;
      this.currentBoulderLog.set(boulderLog);
      this.toastService.showSuccess('Saved successfully', 'Problem Log');
    }
    this.isLoadingModal.set(false);
  }
}

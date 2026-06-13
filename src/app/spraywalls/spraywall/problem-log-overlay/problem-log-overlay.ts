import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { BoulderLogDto, BoulderLogsService, CreateBoulderLogCommand, UpdateBoulderLogCommand } from '@api-net/index';
import { Icon } from 'src/app/core/icon/icon';

@Component({
  selector: 'app-problem-log-overlay',
  imports: [Icon],
  templateUrl: './problem-log-overlay.html',
  styleUrl: './problem-log-overlay.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProblemLogOverlay {
  private boulderLogsService = inject(BoulderLogsService);

  public boulderProblemId = input.required<string>();
  public boulderLog = input<BoulderLogDto>();

  public currentBoulderLog = signal<BoulderLogDto | undefined>(undefined);

  public constructor() {
    effect(() => {
      this.currentBoulderLog.set(this.boulderLog());
    });
  }
  
  public onCreateClick(action: 'sent' | 'project'): void {
    const newLog: CreateBoulderLogCommand = {
      isProject: action === 'project',
      isSent: action === 'sent'
    }
    this.boulderLogsService.createBoulderLogForSpraywall(this.boulderProblemId(), newLog).subscribe({
      next: (boulderLog?: BoulderLogDto) => {
        this.currentBoulderLog.set(boulderLog);
      }
    });
  }

  public onUpdateClick(action: 'sent' | 'project'): void {
    const updateLog: UpdateBoulderLogCommand = {
      isProject: this.currentBoulderLog()!.isProject,
      isSent: this.currentBoulderLog()!.isSent,
      id: this.currentBoulderLog()!.id,
      version: this.currentBoulderLog()!.version
    }

    if (action === 'project') {
      updateLog.isProject = !updateLog.isProject;
    } else {
      updateLog.isSent = !updateLog.isSent;
    }

    this.boulderLogsService.updateBoulderLog(this.currentBoulderLog()!.id, updateLog).subscribe({
      next: (boulderLog?: BoulderLogDto) => {
        this.currentBoulderLog.set(boulderLog);
      }
    });
  }

}

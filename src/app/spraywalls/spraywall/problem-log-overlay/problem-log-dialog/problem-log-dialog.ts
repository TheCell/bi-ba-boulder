import { ChangeDetectionStrategy, Component, effect, inject, OnDestroy, output, signal } from '@angular/core';
import { ProblemLogDialogData } from './problem-log-dialog-data.interface';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BoulderLogDto, BoulderLogsService, CreateBoulderLogCommand, UpdateBoulderLogCommand } from '@api-net/index';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';
import { disabled, form, FormField } from '@angular/forms/signals';
import { Icon } from '../../../../core/icon/icon';
import { IModal } from '../../../../core/modal/modal/modal.interface';
import { CloseModalEvent } from '../../../../core/modal/modal/close-modal-event';
import { StarRating } from '../../../../core/special-form-fields/star-rating/star-rating';

interface IProblemLogForm {
  id?: string;
  version?: number;
  isSent: boolean;
  isProject: boolean;
  rating: number | null;
  fontGrade: string;
}

@Component({
  selector: 'app-problem-log-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Icon, FormField, StarRating],
  templateUrl: './problem-log-dialog.html',
  styleUrl: './problem-log-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProblemLogDialog implements IModal, OnDestroy {
  private boulderLogsService = inject(BoulderLogsService);
  public closeModal = output<CloseModalEvent>();

  private isDisabled = signal<boolean>(false);
  private saveModel = signal<IProblemLogForm>({
    id: undefined,
    isSent: false,
    isProject: false,
    fontGrade: '',
    rating: null,
    version: undefined
  });
  private problemLog?: BoulderLogDto;
  private spraywallProblemId = '';
  private subscription = new Subscription();

  public canCloseWithoutPermission: boolean;
  public isLoading = false;
  public saveForm = form(this.saveModel, (schemaPath) => {
    disabled(schemaPath.rating, { when: ({ valueOf }) => valueOf(schemaPath.isSent) });
  });

  public constructor() {
    this.canCloseWithoutPermission = true;
    effect(() => {
      // todo change form to not dirty when values are the same as the original values
      this.canCloseWithoutPermission = !this.saveForm().dirty();
    });
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public initialize?(data: ProblemLogDialogData): void {
    this.problemLog = data.problemLog;
    this.spraywallProblemId = data.spraywallProblemId;
    this.saveModel.set({
      id: this.problemLog?.id,
      isSent: this.problemLog?.isSent ?? false,
      isProject: this.problemLog?.isProject ?? false,
      fontGrade: this.problemLog?.fontGrade?.toString() ?? '',
      rating: this.problemLog?.rating ?? null,
      version: this.problemLog?.version
    });
  }

  public onSubmit(): void {
    this.isLoading = true;
    this.isDisabled.set(true);

    // todo delete when all is set to undefined / false

    if (this.problemLog) {
      const updateBoulderLogCommand: UpdateBoulderLogCommand = {
        id: this.problemLog.id,
        version: this.problemLog.version,
        isSent: this.saveModel().isSent,
        isProject: this.saveModel().isProject,
        fontGrade: this.saveModel().fontGrade ? Number(this.saveModel().fontGrade) : undefined,
        rating: this.saveModel().rating ? Number(this.saveModel().rating) : undefined
      };
      this.boulderLogsService.updateBoulderLog(this.problemLog.id, updateBoulderLogCommand).subscribe({
        next: (boulderLog?: BoulderLogDto) => {
          this.canCloseWithoutPermission = true;
          this.closeModal.emit({ closeType: 0, data: boulderLog });
        },
        error: () => {
          this.isLoading = false;
          this.isDisabled.set(false);
        }
      });
    } else {
      const createBoulderLogCommand: CreateBoulderLogCommand = {
        isSent: this.saveModel().isSent,
        isProject: this.saveModel().isProject,
        fontGrade: this.saveModel().fontGrade ? Number(this.saveModel().fontGrade) : undefined,
        rating: this.saveModel().rating ? Number(this.saveModel().rating) : undefined
      };
      this.boulderLogsService.createBoulderLogForSpraywall(this.spraywallProblemId, createBoulderLogCommand).subscribe({
        next: (boulderLog?: BoulderLogDto) => {
          this.canCloseWithoutPermission = true;
          this.closeModal.emit({ closeType: 0, data: boulderLog });
        },
        error: () => {
          this.isLoading = false;
          this.isDisabled.set(false);
        }
      });
    }
  }
}

import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, output, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import {
  CreateSpraywallProblemCommand,
  SpraywallProblemsService,
  SpraywallsService,
  UpdateSpraywallProblemCommand
} from '@api-net/index';
import { SpraywallSaveData } from './spraywall-save-data.interface';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { ToastService } from '../../core/toast-container/toast.service';
import { Icon } from '../../core/icon/icon';
import { disabled, form, FormField } from '@angular/forms/signals';

interface ISpraywallForm {
  name: string;
  description: string;
  fontGrade: string;
  isCircuit: boolean;
  noMatch: boolean;
  freeFeet: boolean;
  version?: number;
}

@Component({
  selector: 'app-spraywall-save-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Icon, FormField],
  templateUrl: './spraywall-save-dialog.html',
  styleUrl: './spraywall-save-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SpraywallSaveDialog implements IModal {
  private spraywallsService = inject(SpraywallsService);
  private spraywallProblemsService = inject(SpraywallProblemsService);
  private toastService = inject(ToastService);
  private isDisabled = signal<boolean>(false);
  private saveModel = signal<ISpraywallForm>({
    name: '',
    description: '',
    fontGrade: '',
    isCircuit: false,
    noMatch: false,
    freeFeet: false,
    version: undefined
  });

  public closeModal = output<CloseModalEvent>();

  public canCloseWithoutPermission = true;
  public isLoading = false;
  public saveForm = form(this.saveModel, (schemaPath) => {
    disabled(schemaPath.name, { when: () => this.isDisabled() });
    disabled(schemaPath.description, { when: () => this.isDisabled() });
    disabled(schemaPath.fontGrade, { when: () => this.isDisabled() });
    disabled(schemaPath.isCircuit, { when: () => this.isDisabled() });
    disabled(schemaPath.noMatch, { when: () => this.isDisabled() });
    disabled(schemaPath.freeFeet, { when: () => this.isDisabled() });
  });
  private imageData?: string;
  private spraywallId = '';
  private problemId = '';

  public constructor() {
    effect(() => {
      this.canCloseWithoutPermission = !this.saveForm().dirty();
    });
  }

  public initialize(data: SpraywallSaveData): void {
    this.imageData = data.imageData;
    this.spraywallId = data.spraywallId;
    this.problemId = data.existingId ?? '';
    this.saveModel.set({
      name: data.name,
      description: data.description ?? '',
      fontGrade: data.fontGrade?.toString() ?? '',
      isCircuit: data.isCircuit ?? false,
      noMatch: data.noMatch ?? false,
      freeFeet: data.freeFeet ?? false,
      version: data.version
    });
  }

  public onSubmit(): void {
    if (this.isNameMissing()) {
      return;
    }

    this.isLoading = true;
    this.isDisabled.set(true);

    if (this.problemId) {
      const update: UpdateSpraywallProblemCommand = {
        name: this.saveModel().name,
        description: this.saveModel().description || undefined,
        image: this.imageData!,
        fontGrade: this.saveModel().fontGrade ? Number(this.saveModel().fontGrade) : undefined,
        isCircuit: this.saveModel().isCircuit,
        noMatch: this.saveModel().noMatch,
        freeFeet: this.saveModel().freeFeet,
        version: this.saveModel().version
      };
      this.spraywallProblemsService.updateProblem(this.problemId, update).subscribe({
        next: () => {
          this.isLoading = false;
          this.canCloseWithoutPermission = true;
          this.toastService.showSuccess('Updated Successfully', 'You have successfully updated the spraywall.');
          this.closeModal.emit({ closeType: 0 });
        },
        error: () => {
          this.isLoading = false;
          this.isDisabled.set(false);
          this.canCloseWithoutPermission = false;
        }
      });
    } else {
      const postRegisterRequest: CreateSpraywallProblemCommand = {
        name: this.saveModel().name,
        description: this.saveModel().description || undefined,
        image: this.imageData!,
        fontGrade: this.saveModel().fontGrade ? Number(this.saveModel().fontGrade) : undefined,
        isCircuit: this.saveModel().isCircuit,
        noMatch: this.saveModel().noMatch,
        freeFeet: this.saveModel().freeFeet
      };

      this.spraywallsService.createSpraywallProblem(this.spraywallId, postRegisterRequest).subscribe({
        next: () => {
          this.isLoading = false;
          this.canCloseWithoutPermission = true;
          this.closeModal.emit({ closeType: 0 });
          this.toastService.showSuccess('Saved Successfully', 'You have successfully saved the spraywall.');
        },
        error: () => {
          this.isLoading = false;
          this.isDisabled.set(false);
          this.canCloseWithoutPermission = false;
        }
      });
    }
  }

  public isNameMissing(): boolean {
    return !this.saveModel().name.trim();
  }
}

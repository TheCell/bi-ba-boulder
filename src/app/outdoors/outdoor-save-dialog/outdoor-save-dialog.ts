import { Component, computed, effect, inject, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { OutdoorSaveData } from './outdoor-save-data.interface';
import { disabled, form, FormField, minLength, required } from '@angular/forms/signals';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { CreateLineCommand, LineData, LineDto, UpdateLineCommand } from '@api-net/model/models';
import { IModal } from '../../core/modal/modal/modal.interface';
import { LinesService } from '@api-net/index';
import { ToastService } from '../../core/toast-container/toast.service';
import { Icon } from '../../core/icon/icon';

interface IOutdoorForm {
  name: string;
  description: string;
  fontGrade: string;
  identifier: string;
  lineId?: string;
  version?: number;
}

@Component({
  selector: 'app-outdoor-save-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Icon, FormField],
  templateUrl: './outdoor-save-dialog.html',
  styleUrl: './outdoor-save-dialog.scss'
})
export class OutdoorSaveDialog implements IModal {
  private linesService = inject(LinesService);
  private toastService = inject(ToastService);

  private isDisabled = signal<boolean>(false);
  private saveModel = signal<IOutdoorForm>({
    name: '',
    identifier: '',
    description: '',
    fontGrade: '',
    lineId: undefined,
    version: undefined
  });

  public closeModal = output<CloseModalEvent>();

  public canCloseWithoutPermission = true;
  public isLoading = false;
  public isSubmitDisabled = computed(() => this.isLoading || this.saveForm().disabled() || this.saveForm().invalid());
  public saveForm = form(this.saveModel, (schemaPath) => {
    disabled(schemaPath.name, { when: () => this.isDisabled() });
    disabled(schemaPath.description, { when: () => this.isDisabled() });
    disabled(schemaPath.fontGrade, { when: () => this.isDisabled() });
    disabled(schemaPath.identifier, { when: () => this.isDisabled() });
    required(schemaPath.name);
    minLength(schemaPath.name, 5);
    required(schemaPath.fontGrade);
    required(schemaPath.identifier);
    minLength(schemaPath.identifier, 1);
  });
  private lineData?: LineData;
  private blocId = '';

  public constructor() {
    effect(() => {
      this.canCloseWithoutPermission = !this.saveForm().dirty();
    });
  }

  public initialize(data: OutdoorSaveData): void {
    this.lineData = data.lineData;
    this.blocId = data.blocId;

    console.log('initialize', data);

    this.saveModel.set({
      name: data.name ?? '',
      description: data.description ?? '',
      fontGrade: data.fontGrade?.toString() ?? '',
      identifier: data.identifier ?? '',
      lineId: data.existingId,
      version: data.version
    });

    console.log(data);
  }

  public onSubmit(): void {
    if (this.saveForm().invalid()) {
      return;
    }

    this.isLoading = true;
    this.isDisabled.set(true);

    const lineId = this.saveModel().lineId;
    if (lineId !== undefined && lineId !== null) {
      const updateLineCommand: UpdateLineCommand = {
        identifier: this.saveModel().identifier,
        name: this.saveModel().name,
        description: this.saveModel().description || undefined,
        fontGrade: this.saveModel().fontGrade ? Number(this.saveModel().fontGrade) : undefined,
        data: this.lineData!,
        version: this.saveModel().version
      };

      this.linesService.updateLine(lineId, updateLineCommand).subscribe({
        next: (_: LineDto) => {
          this.isLoading = false;
          this.canCloseWithoutPermission = true;
          this.toastService.showSuccess('Updated Successfully', 'You have successfully updated the line.');
          this.closeModal.emit({ closeType: 0 });
        },
        error: () => {
          this.isLoading = false;
          this.isDisabled.set(false);
          this.canCloseWithoutPermission = false;
        }
      });
    } else {
      const postRegisterRequest: CreateLineCommand = {
        blocId: this.blocId,
        identifier: this.saveModel().identifier,
        name: this.saveModel().name,
        description: this.saveModel().description || undefined,
        fontGrade: this.saveModel().fontGrade ? Number(this.saveModel().fontGrade) : undefined,
        data: this.lineData!
      };

      this.linesService.createLineForBloc(this.blocId, postRegisterRequest).subscribe({
        next: (_: LineDto) => {
          this.isLoading = false;
          this.canCloseWithoutPermission = true;
          this.closeModal.emit({ closeType: 0 });
          this.toastService.showSuccess('Saved Successfully', 'You have successfully saved the line.');
        },
        error: (_: Error) => {
          this.isDisabled.set(false);
          this.canCloseWithoutPermission = false;
          this.isLoading = false;
        }
      });
    }
  }

}


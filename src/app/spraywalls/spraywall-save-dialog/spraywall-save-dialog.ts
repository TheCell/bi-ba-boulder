import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy, output } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CreateSpraywallProblemCommand, SpraywallProblemsService, SpraywallsService, UpdateSpraywallProblemCommand } from '@api-net/index';
import { Icon } from 'src/app/core/icon/icon';
import { IModal } from 'src/app/core/modal/modal/modal.interface';
import { ToastService } from 'src/app/core/toast-container/toast.service';
import { SpraywallSaveData } from './spraywall-save-data.interface';
import { Subscription } from 'rxjs';
import { CloseModalEvent } from 'src/app/core/modal/modal/close-modal-event';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
interface ISpraywallForm extends Omit<UpdateSpraywallProblemCommand, "image"> { }

@Component({
  selector: 'app-spraywall-save-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Icon],
  templateUrl: './spraywall-save-dialog.html',
  styleUrl: './spraywall-save-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallSaveDialog implements IModal, OnDestroy {
  private _fb = inject(FormBuilder);
  private spraywallsService = inject(SpraywallsService);
  private spraywallProblemsService = inject(SpraywallProblemsService);
  private toastService = inject(ToastService);
  
  public closeModal = output<CloseModalEvent>();
  
  public canCloseWithoutPermission = true;
  public isLoading = false;
  public saveForm = this._fb.nonNullable.group<ISpraywallForm>({
    name: '',
    description: undefined,
    fontGrade: undefined,
    version: undefined,
  });

  private imageData?: string;
  private spraywallId = '';
  private problemId = '';
  private subscription = new Subscription();

  public constructor() {
    this.saveForm.controls.name.addValidators([Validators.required]);

    this.subscription.add(this.saveForm.valueChanges.subscribe(() => {
      if (this.saveForm.dirty) {
        this.canCloseWithoutPermission = false;
      } else {
        this.canCloseWithoutPermission = true;
      }
    }));
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public initialize(data: SpraywallSaveData): void {
    console.log(data satisfies SpraywallSaveData);
    
    this.imageData = data.imageData;
    this.spraywallId = data.spraywallId;
    this.problemId = data.existingId ?? '';
    this.saveForm.controls.name.setValue(data.name);
    this.saveForm.controls.fontGrade?.setValue(data.fontGrade === undefined ? undefined : data.fontGrade);
    this.saveForm.controls.description?.setValue(data.description);
    this.saveForm.controls.version?.setValue(data.version === undefined ? undefined : data.version);
  }

  public onSubmit(): void {
    this.isLoading = true;
    this.saveForm.disable();

    if (this.problemId) {
      const update: UpdateSpraywallProblemCommand = {
        name: this.saveForm.controls.name.value!,
        description: this.saveForm.controls.description?.value,
        image: this.imageData!,
        fontGrade: this.saveForm.controls.fontGrade?.value,
        version: this.saveForm.controls.version?.value,
      }
      this.spraywallProblemsService.updateProblem(this.problemId, update).subscribe({
        next: () => {
          this.isLoading = false;
          this.saveForm.reset();
          this.toastService.showSuccess('Updated Successfully', 'You have successfully updated the spraywall.');
          this.closeModal.emit({ closeType: 0 });
        },
        error: () => {
          this.isLoading = false;
          this.saveForm.enable();
          this.canCloseWithoutPermission = false;
        }
      });
    } else {
      const postRegisterRequest: CreateSpraywallProblemCommand = {
        name: this.saveForm.controls.name.value!,
        description: this.saveForm.controls.description?.value,
        image: this.imageData!,
        fontGrade: this.saveForm.controls.fontGrade?.value,
      };
  
      this.spraywallsService.createSpraywallProblem(this.spraywallId, postRegisterRequest).subscribe({
        next: () => {
          this.isLoading = false;
          this.saveForm.reset();
          this.closeModal.emit({ closeType: 0 });
          this.toastService.showSuccess('Saved Successfully', 'You have successfully saved the spraywall.');
        },
        error: () => {
          this.isLoading = false;
          this.saveForm.enable();
          this.canCloseWithoutPermission = false;
        }
      });
    }
  }
}

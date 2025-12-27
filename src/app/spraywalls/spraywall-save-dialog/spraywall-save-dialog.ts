import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy, output } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { PutCreateRequest, SpraywallProblemsService, SpraywallsService } from '@api/index';
import { Icon } from 'src/app/core/icon/icon';
import { IModal } from 'src/app/core/modal/modal/modal.interface';
import { ToastService } from 'src/app/core/toast-container/toast.service';
import { SpraywallSaveData } from './spraywall-save-data.interface';
import { Subscription } from 'rxjs';
import { CloseModalEvent } from 'src/app/core/modal/modal/close-modal-event';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
interface ISpraywallForm extends Omit<PutCreateRequest, "image"> { }

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
  public saveForm = this._fb.group<ISpraywallForm>({
    name: '',
    description: undefined,
    fontGrade: null
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
    this.saveForm.controls.fontGrade?.setValue(data.fontGrade);
    this.saveForm.controls.description?.setValue(data.description);
  }

  public onSubmit(): void {
    this.isLoading = true;
    this.saveForm.disable();

    if (this.problemId) {
      const update: PutCreateRequest = {
        name: this.saveForm.controls.name.value!,
        description: this.saveForm.controls.description?.value,
        image: this.imageData!,
        fontGrade: this.saveForm.controls.fontGrade?.value
      }
      this.spraywallProblemsService.postUpdateSpraywallProblem(this.problemId, update).subscribe({
        next: () => {
          this.isLoading = false;
          this.saveForm.reset();
          this.closeModal.emit({ closeType: 0 });
          this.toastService.showSuccess('Updated Successfully', 'You have successfully updated the spraywall.');
        },
        error: () => {
          this.isLoading = false;
          this.saveForm.enable();
          this.canCloseWithoutPermission = false;
        }
      });
    } else {
      const postRegisterRequest: PutCreateRequest = {
        name: this.saveForm.controls.name.value!,
        description: this.saveForm.controls.description?.value,
        image: this.imageData!,
        fontGrade: this.saveForm.controls.fontGrade?.value,
      };
  
      this.spraywallsService.putCreate(this.spraywallId, postRegisterRequest).subscribe({
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

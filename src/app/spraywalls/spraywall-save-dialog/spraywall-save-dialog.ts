import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { PutCreateRequest, SpraywallsService } from '@api/index';
import { Icon } from 'src/app/core/icon/icon';
import { ModalService } from 'src/app/core/modal/modal.service';
import { iModal } from 'src/app/core/modal/modal/modal.interface';
import { ToastService } from 'src/app/core/toast-container/toast.service';
import { SpraywallSaveData } from './spraywall-save-data.interface';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
interface ISpraywallForm extends Omit<PutCreateRequest, "image"> { }

@Component({
  selector: 'app-spraywall-save-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Icon],
  templateUrl: './spraywall-save-dialog.html',
  styleUrl: './spraywall-save-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallSaveDialog implements iModal {
  private _fb = inject(FormBuilder);
  private spraywallsService = inject(SpraywallsService);
  private modalService = inject(ModalService);
  private toastService = inject(ToastService);
  
  public canCloseWithoutPermission = true;
  public isLoading = false;
  public saveForm = this._fb.group<ISpraywallForm>({
    name: '',
    description: undefined,
    fontGrade: null
  });

  private imageData?: string;
  private spraywallId = '';

  public constructor() {
    this.saveForm.controls.name.addValidators([Validators.required])
    this.saveForm.controls.fontGrade.addValidators([Validators.required])
  }

  public initialize(data: SpraywallSaveData): void {
    console.log(data satisfies SpraywallSaveData);
    
    this.imageData = data.imageData;
    this.spraywallId = data.spraywallId;
  }

  public onSubmit(): void {
    this.isLoading = true;
    this.saveForm.disable();
    const postRegisterRequest: PutCreateRequest = {
      name: this.saveForm.controls.name.value!,
      description: this.saveForm.controls.description?.value,
      image: this.imageData!,
      fontGrade: this.saveForm.controls.fontGrade?.value,
      // password: this.saveForm.controls.password.value,
    };

    console.log(postRegisterRequest);
    this.spraywallsService.putCreate(this.spraywallId, postRegisterRequest).subscribe({
      next: () => {
        this.isLoading = false;
        this.saveForm.reset();
        this.modalService.close();
        this.toastService.showSuccess('Saved Successfully', 'You have successfully saved the spraywall.');
      },
      error: () => {
        this.isLoading = false;
        this.saveForm.enable();
      }
    });
    
    // this.spraywallsService.putCreate()
    // this.authService.postRegister(postRegisterRequest).subscribe({
    //   next: () => {
    //     this.isLoading = false;
    //     this.saveForm.reset();
    //     this.modalService.close();
    //     this.toastService.showSuccess('Registration Successful', 'You have successfully registered. Please check your email to verify your account.');
    //   },
    //   error: () => {
    //     this.isLoading = false;
    //     this.saveForm.enable();
    //   }
    // });
  }
}

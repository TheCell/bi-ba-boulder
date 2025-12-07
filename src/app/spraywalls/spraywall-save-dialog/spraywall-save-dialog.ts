import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PutCreateRequest, SpraywallsService } from '@api/index';
import { Icon } from 'src/app/core/icon/icon';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
interface ISpraywallForm extends Omit<PutCreateRequest, "image" | "tempPwd"> { }

@Component({
  selector: 'app-spraywall-save-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Icon],
  templateUrl: './spraywall-save-dialog.html',
  styleUrl: './spraywall-save-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallSaveDialog {
  private _fb = inject(NonNullableFormBuilder);
  private spraywallsService = inject(SpraywallsService);
  
  public canCloseWithoutPermission = true;
  public isLoading = false;
  public saveForm = this._fb.group<ISpraywallForm>({
    name: '',
    description: ''
  });

  public constructor() {
    this.saveForm.controls.name.addValidators([Validators.required])
  }

  public onSubmit(): void {
    this.isLoading = true;
    this.saveForm.disable();
    // const postRegisterRequest: PutCreateRequest = {
    //   name: this.saveForm.controls.name.value,
    //   description: this.saveForm.controls.description?.value,
    //   image: 'todo',
    //   tempPwd: 'todo'
    //   // password: this.saveForm.controls.password.value,
    // };

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

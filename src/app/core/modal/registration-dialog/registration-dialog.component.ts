import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { AuthService, PostRegisterRequest } from '@api/index';
import { ModalService } from '../modal.service';
import { iModal } from '../modal/modal.interface';
import { Subscription } from 'rxjs';
import { Icon } from '../../icon/icon';
import { ToastService } from '../../toast-container/toast.service';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
interface IRegistrationForm extends PostRegisterRequest { }

@Component({
  selector: 'app-registration-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Icon],
  templateUrl: './registration-dialog.component.html',
  styleUrl: './registration-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class RegistrationDialogComponent implements iModal, OnDestroy {
  private _fb = inject(NonNullableFormBuilder);
  private authService = inject(AuthService);
  private modalService = inject(ModalService);
  private toastService = inject(ToastService);
  
  public canCloseWithoutPermission = true;
  public isLoading = false;
  public registrationForm = this._fb.group<IRegistrationForm>({
    username: '',
    email: '',
    password: '',
  });

  private subscription: Subscription = new Subscription();
  
  public constructor() {
    this.registrationForm.controls.email.addValidators([Validators.required, Validators.email]);
    this.registrationForm.controls.username.addValidators([Validators.required, Validators.minLength(3)]);
    this.registrationForm.controls.password.addValidators([Validators.required, Validators.minLength(8)]);

    this.registrationForm.valueChanges.subscribe(() => {
      if (this.registrationForm.controls.email.value || this.registrationForm.controls.username.value || this.registrationForm.controls.password.value) {
        this.canCloseWithoutPermission = false;
      } else {
        this.canCloseWithoutPermission = true;
      }
    });
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public onSubmit(): void {
    this.isLoading = true;
    this.registrationForm.disable();
    const postRegisterRequest: PostRegisterRequest = {
      email: this.registrationForm.controls.email.value,
      username: this.registrationForm.controls.username.value,
      password: this.registrationForm.controls.password.value,
    };

    this.authService.postRegister(postRegisterRequest).subscribe({
      next: () => {
        this.isLoading = false;
        this.registrationForm.reset();
        this.modalService.close(0);
        this.toastService.showSuccess('Registration Successful', 'You have successfully registered. Please check your email to verify your account.');
      },
      error: () => {
        this.isLoading = false;
        this.registrationForm.enable();
      }
    });
  }
}

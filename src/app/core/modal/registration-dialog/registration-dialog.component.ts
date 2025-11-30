import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { AuthService, PostRegisterRequest } from '@api/index';
import { ModalService } from '../modal.service';
import { IStopClosing } from '../modal/I-stop-closing';
import { Subscription } from 'rxjs';
import { Icon } from '../../icon/icon';

interface IloginForm extends PostRegisterRequest { }

@Component({
  selector: 'app-registration-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Icon],
  templateUrl: './registration-dialog.component.html',
  styleUrl: './registration-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class RegistrationDialogComponent implements IStopClosing, OnDestroy {
  private _fb = inject(NonNullableFormBuilder);
  private authService = inject(AuthService);
  private modalService = inject(ModalService);
  
  public canCloseWithoutPermission: boolean = true;
  public isLoading = false;
  public loginForm = this._fb.group<IloginForm>({
    username: '',
    email: '',
    password: '',
  });

  private subscription: Subscription = new Subscription();
  
  public constructor() {
    this.loginForm.controls.email.addValidators([Validators.required, Validators.email]);
    this.loginForm.controls.username.addValidators([Validators.required, Validators.minLength(3)]);
    this.loginForm.controls.password.addValidators([Validators.required, Validators.minLength(8)]);

    this.loginForm.valueChanges.subscribe(() => {
      this.canCloseWithoutPermission = false;
    });
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public onSubmit(): void {
    this.isLoading = true;
    this.loginForm.disable();
    const postRegisterRequest: PostRegisterRequest = {
      email: this.loginForm.controls.email.value,
      username: this.loginForm.controls.username.value,
      password: this.loginForm.controls.password.value,
    };

    this.authService.postRegister(postRegisterRequest).subscribe({
      next: () => {
        this.isLoading = false;
        this.modalService.close();
        console.log('Registration successful');
        this.loginForm.enable();
      },
      error: () => {
        this.isLoading = false;
        console.log('Registration failed');
        this.loginForm.enable();
      }
    });
  }
}

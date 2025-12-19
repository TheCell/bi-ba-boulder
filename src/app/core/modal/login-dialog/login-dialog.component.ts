import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { AuthService, PostAppAuthLoginRequest, TokenDto } from '@api/index';
import { iModal } from '../modal/modal.interface';
import { Subscription } from 'rxjs';
import { Icon } from '../../icon/icon';
import { ToastService } from '../../toast-container/toast.service';
import { ModalService } from '../modal.service';
import { LoginTrackerService } from 'src/app/auth/login-tracker.service';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
interface IloginForm extends PostAppAuthLoginRequest { }

@Component({
  selector: 'app-login-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Icon],
  templateUrl: './login-dialog.component.html',
  styleUrl: './login-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class LoginDialogComponent implements iModal, OnDestroy {
  private _fb = inject(NonNullableFormBuilder);
  private loginTrackerService = inject(LoginTrackerService);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private modalService = inject(ModalService);

  public canCloseWithoutPermission = true;
  public isLoading = false;
  public loginForm = this._fb.group<IloginForm>({
    email: (''),
    password: (''),
  });

  private subscription: Subscription = new Subscription();
  
  public constructor() {
    this.loginForm.controls.email.addValidators([Validators.required,Validators.email]);
    this.loginForm.controls.password.addValidators([Validators.required]);
    
    this.loginForm.valueChanges.subscribe(() => {
      if (this.loginForm.controls.email.value || this.loginForm.controls.password.value) {
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
    this.loginForm.disable();
    const loginRequest: PostAppAuthLoginRequest = {
      email: this.loginForm.controls.email.value,
      password: this.loginForm.controls.password.value,
    };

    this.authService.postAppAuthLogin(loginRequest).subscribe({
      next: (token: TokenDto) => {
        this.loginTrackerService.saveLoginInformation(token);

        this.isLoading = false;
        this.loginForm.reset();
        this.modalService.close(0);
        this.toastService.showSuccess('Login Successful', 'You have successfully logged in!');
      },
      error: () => {
        this.isLoading = false;
        this.loginForm.enable();
      }
    });
  }
}

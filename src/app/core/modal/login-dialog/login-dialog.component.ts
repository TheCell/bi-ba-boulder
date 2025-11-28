import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { AuthService, PostAppAuthLoginRequest } from '@api/index';

interface IloginForm extends PostAppAuthLoginRequest { }

@Component({
  selector: 'app-login-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './login-dialog.component.html',
  styleUrl: './login-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginDialogComponent {
  private _fb = inject(NonNullableFormBuilder);
  private s = inject(AuthService)

  public loginForm = this._fb.group<IloginForm>({
    email: (''),
    password: (''),
  });
  
  public constructor() {
    this.loginForm.controls.email.addValidators([Validators.email]);
    this.loginForm.controls.password.addValidators([Validators.required]);
  }

  public onSubmit(): void {
    console.log('loginForm', this.loginForm.value);
  }
}

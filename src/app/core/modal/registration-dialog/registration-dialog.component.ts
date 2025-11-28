import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { AuthService, PostRegisterRequest } from '@api/index';

interface IloginForm extends PostRegisterRequest { }

@Component({
  selector: 'app-registration-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './registration-dialog.component.html',
  styleUrl: './registration-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class RegistrationDialogComponent {
  private _fb = inject(NonNullableFormBuilder);
  private authService = inject(AuthService);
  
  public isLoading = false;
  public loginForm = this._fb.group<IloginForm>({
    // username: '',
    email: '',
    password: '',
  });
  
  public constructor() {
    this.loginForm.controls.email.addValidators([Validators.email]);
    this.loginForm.controls.password.addValidators([Validators.required]);
  }

  public onSubmit(): void {
    this.isLoading = true;
    const postRegisterRequest: PostRegisterRequest = {
      email: this.loginForm.controls.email.value,
      // username: this.loginForm.controls.username.value,
      password: this.loginForm.controls.password.value,
    };

    this.authService.postRegister(postRegisterRequest).subscribe({
      next: () => {
        this.isLoading = false;
        console.log('Registration successful');
      },
      error: () => {
        this.isLoading = false;
        console.log('Registration failed');
      }
    });
  }
}

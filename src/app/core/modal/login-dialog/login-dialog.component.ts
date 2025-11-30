import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, FormsModule, Validators } from '@angular/forms';
import { AuthService, PostAppAuthLoginRequest } from '@api/index';
import { IStopClosing } from '../modal/I-stop-closing';
import { Subscription } from 'rxjs';

interface IloginForm extends PostAppAuthLoginRequest { }

@Component({
  selector: 'app-login-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './login-dialog.component.html',
  styleUrl: './login-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginDialogComponent implements IStopClosing, OnDestroy {
  private _fb = inject(NonNullableFormBuilder);
  private s = inject(AuthService)
  public canCloseWithoutPermission: boolean = true;

  public loginForm = this._fb.group<IloginForm>({
    email: (''),
    password: (''),
  });

  private subscription: Subscription = new Subscription();
  
  public constructor() {
    this.loginForm.controls.email.addValidators([Validators.email]);
    this.loginForm.controls.password.addValidators([Validators.required]);
    
    this.loginForm.valueChanges.subscribe(() => {
      this.canCloseWithoutPermission = false;
    });
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public onSubmit(): void {
    console.log('loginForm', this.loginForm.value);
  }
}

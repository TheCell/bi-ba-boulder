import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormsModule, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Icon } from '../icon/icon';
import { LoginTrackerService } from 'src/app/auth/login-tracker.service';
import { ToastService } from '../toast-container/toast.service';
import { FeedbacksService, PostSendFeedbackRequest } from '@api/index';

// interface IFeedbackForm extends PostAppAuthLoginRequest { }
interface IFeedbackForm {
  email: string,
  feedback: string
}

@Component({
  selector: 'app-feedback-overlay',
  imports: [NgClass, FormsModule, ReactiveFormsModule, Icon],
  templateUrl: './feedback-overlay.html',
  styleUrl: './feedback-overlay.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class FeedbackOverlay implements OnInit {
  private _fb = inject(NonNullableFormBuilder);
  public loginTrackerService = inject(LoginTrackerService);
  private feedbacksService = inject(FeedbacksService);
  private toastService = inject(ToastService);
  public isFormOpen = false;
  public isLoading = false;
  public uniqueId = ''.appendUniqueId();
  
  public feedbackForm = this._fb.group<IFeedbackForm>({
    email: (''),
    feedback: (''),
  });
  
  // private feedbackForm = viewChild<ElementRef>('feedbackForm');

  public constructor() {
    // this.feedbackForm.controls.email.addValidators([Validators.required, Validators.email]);
    this.feedbackForm.controls.feedback.addValidators([Validators.required, Validators.minLength(10)]);
  }

  public ngOnInit(): void {
    console.log('test');
    
  }

  public openFeedback(): void {
    // console.log('click');
    // console.log(this.feedbackForm());
    if (this.loginTrackerService.isLoggedIn()) {
      this.feedbackForm.controls.email.setValue('' + this.loginTrackerService.getUserMail());
      this.feedbackForm.enable();
      this.feedbackForm.controls.email.disable();
    } else {
      this.feedbackForm.disable();
    }

    this.isFormOpen = true;
  }

  public closeFeedback(): void {
    this.isFormOpen = false;
  }
  
  public onSubmit(): void {
    this.isLoading = true;
    this.feedbackForm.disable();

    const feedbackRequest: PostSendFeedbackRequest = {
      feedback: this.feedbackForm.controls.feedback.value
    };

    this.feedbacksService.postSendFeedback(feedbackRequest).subscribe({
      next: () => {
        this.feedbackForm.reset();
        this.feedbackForm.enable();
        this.toastService.showSuccess('Login Successful', 'You have successfully logged in!');
        this.isLoading = false;
        this.closeFeedback();
      },
      error: () => {
        this.isLoading = false;
        this.feedbackForm.enable();
      }
    });
    // const loginRequest: PostAppAuthLoginRequest = {
    //   email: this.loginForm.controls.email.value,
    //   password: this.loginForm.controls.password.value,
    // };

    // this.authService.postAppAuthLogin(loginRequest).subscribe({
    //   next: (token: TokenDto) => {
    //     this.loginTrackerService.saveLoginInformation(token);

    //     this.isLoading = false;
    //     this.loginForm.reset();
    //     this.closeModal.emit({ closeType: 0 });
    //     this.toastService.showSuccess('Login Successful', 'You have successfully logged in!');
    //   },
    //   error: () => {
    //     this.isLoading = false;
    //     this.loginForm.enable();
    //   }
    // });
  }
}

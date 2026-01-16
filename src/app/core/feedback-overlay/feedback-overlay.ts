import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormsModule, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Icon } from '../icon/icon';
import { LoginTrackerService } from 'src/app/auth/login-tracker.service';
import { ToastService } from '../toast-container/toast.service';
import { FeedbacksService, PostSendFeedbackRequest } from '@api/index';

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
  
  public constructor() {
    this.feedbackForm.controls.feedback.addValidators([Validators.required, Validators.minLength(10)]);
  }

  public ngOnInit(): void {
    console.log('test');
    
  }

  public openFeedback(): void {
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
        this.toastService.showSuccess('Feedback Sent Successful', 'Your feedback has been sent. A copy was sent to your address.');
        this.isLoading = false;
        this.closeFeedback();
      },
      error: () => {
        this.isLoading = false;
        this.feedbackForm.enable();
      }
    });
  }
}

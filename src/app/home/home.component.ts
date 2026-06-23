import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FeedbackOverlay } from '../core/feedback-overlay/feedback-overlay';

@Component({
  selector: 'app-home',
  imports: [RouterLink, FeedbackOverlay],
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './home.component.scss'
})
export class HomeComponent {}

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Toast } from './toast/toast';
import { ToastService } from './toast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-toast-container',
  imports: [Toast],
  templateUrl: './toast-container.html',
  styleUrl: './toast-container.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastContainer implements OnInit, OnDestroy {
  private changeDetectorRef = inject(ChangeDetectorRef);
  public toastService = inject(ToastService);

  private subscription = new Subscription();
  
  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public ngOnInit(): void {
    this.subscription.add(this.toastService.arrayUpdated.subscribe({
      next: () => {
        this.changeDetectorRef.markForCheck();
      }
    }));
  }

}

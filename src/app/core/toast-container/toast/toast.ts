import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { ToastService } from '../toast.service';
import { IToastInternal } from '../I-toast-internal';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-toast',
  imports: [NgClass],
  templateUrl: './toast.html',
  styleUrl: './toast.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class Toast implements OnChanges, OnDestroy {
  public toastService = inject(ToastService);
  private changeDetection = inject(ChangeDetectorRef); // todo remove after fixing change detection issue

  public toast = input.required<IToastInternal>();

  public id: string = ''.appendUniqueId();
  public showFadeout = false;
  
  // todo messages don't dissapear because of change detection. Update with most recent signals
  private delay?: number;
  private fadeoutDelayTimer?: number;

  public ngOnChanges(changes: SimpleChanges): void {
    const toast = changes['toast'];

    if (toast.previousValue) {
      this.stopDelay();
    }
    
    if (toast.currentValue) {
      if (toast.currentValue.delay) {
        this.startDelay();
      }
    }
  }

  public ngOnDestroy(): void {
    if (this.delay) {
      window.clearTimeout(this.delay);
    }
    if (this.fadeoutDelayTimer) {
      window.clearTimeout(this.fadeoutDelayTimer);
    }
  }

  public stopDelay(): void {
    if (this.delay) {
      window.clearTimeout(this.delay);
    }
    if (this.fadeoutDelayTimer) {
      window.clearTimeout(this.fadeoutDelayTimer);
    }
    this.showFadeout = false;
  }

  public startDelay(): void {
    const delay = this.toast().delay;
    if (delay && delay > 0) {
      this.fadeoutDelayTimer = window.setTimeout(() => {
        this.showFadeout = true;
        this.changeDetection.markForCheck(); // todo remove after fixing change detection issue
      }, delay - 500);

      this.delay = window.setTimeout(() => {
        this.toastService.remove(this.toast().id);
        this.changeDetection.markForCheck(); // todo remove after fixing change detection issue
      }, delay);
    }
  }
}

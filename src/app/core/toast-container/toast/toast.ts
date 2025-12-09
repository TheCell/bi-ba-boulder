import { ChangeDetectionStrategy, Component, inject, input, OnChanges, OnDestroy, SimpleChanges } from '@angular/core';
import { ToastService } from '../toast.service';
import { IToastInternal } from '../I-toast-internal';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-toast',
  imports: [NgClass],
  templateUrl: './toast.html',
  styleUrl: './toast.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class Toast implements OnChanges, OnDestroy {
  public toastService = inject(ToastService);

  public toast = input.required<IToastInternal>();

  public id: string = ''.appendUniqueId();
  public showFadeout: boolean = false;
  
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
      }, delay - 500);

      this.delay = window.setTimeout(() => {
        this.toastService.remove(this.toast().id);
      }, delay);
    }
  }
}

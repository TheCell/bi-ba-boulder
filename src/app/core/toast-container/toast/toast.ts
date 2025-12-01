import { ChangeDetectionStrategy, Component, inject, input, OnChanges, SimpleChanges } from '@angular/core';
import { ToastService } from '../toast.service';
import { IToastInternal } from '../I-toast-internal';

@Component({
  selector: 'app-toast',
  imports: [],
  templateUrl: './toast.html',
  styleUrl: './toast.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class Toast implements OnChanges {
  public toastService = inject(ToastService);

  public toast = input.required<IToastInternal>();

  public id: string = ''.appendUniqueId();
  
  // private delay?: NodeJS.Timeout;

  public ngOnChanges(changes: SimpleChanges): void {
    const toast = changes['toast'];
    if (toast.currentValue) {
      if (toast.currentValue.delay) {
        // this.delay = setTimeout(() => {
        //   this.toastService.remove(toast.currentValue.id);
        //   clearTimeout(this.delay);
        // }, toast.currentValue.delay);
      }
    }
  }
}

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Toast } from './toast/toast';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-container',
  imports: [Toast],
  templateUrl: './toast-container.html',
  styleUrl: './toast-container.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastContainer {
  public toastService = inject(ToastService);

}

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from './toast.service';
import { JsonPipe, NgClass } from '@angular/common';

@Component({
  selector: 'app-toast',
  imports: [JsonPipe, NgClass],
  templateUrl: './toast.html',
  styleUrl: './toast.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class Toast {
  public toastService = inject(ToastService);
  
  public id: string = ''.appendUniqueId();
  public isDanger = false;
}

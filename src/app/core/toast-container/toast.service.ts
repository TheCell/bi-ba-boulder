import { Injectable } from '@angular/core';
import { Toast } from './toast/toast';
import { IToast } from './toast/I-toast';
import { IToastInternal } from './I-toast-internal';



@Injectable({
  providedIn: 'root'
})
export class ToastService {
  public toasts: IToastInternal[] = [];

  private standardDelay: number = 3000;

  public constructor() { }

  public show(toast: IToast): void {
    const id = ''.appendUniqueId();
    this.toasts.push({ ...toast, id: id });
  }

  public showInfo(title: string, message: string, delay?: number): void {
    this.show({ title, message, delay: delay ?? this.standardDelay });
  }

  public showSuccess(title: string, message: string, delay?: number): void {
    this.show({ title, message, classname: 'success', delay: delay ?? this.standardDelay });
  }

  public showDanger(title: string, message: string): void {
    this.show({ title, message, classname: 'danger' });
  }

  public showWarning(title: string, message: string, delay?: number): void {
    this.show({ title, message, classname: 'warning', delay: delay ?? this.standardDelay });
  }

  public remove(id: string): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
  }

}

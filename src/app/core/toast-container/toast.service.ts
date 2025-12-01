import { Injectable } from '@angular/core';
import { Toast } from './toast/toast';
import { IToast } from './toast/I-toast';
import { IToastInternal } from './I-toast-internal';



@Injectable({
  providedIn: 'root'
})
export class ToastService {
  public toasts: IToastInternal[] = [];

  public constructor() { }

  public show(toast: IToast): void {
    const id = ''.appendUniqueId();
    this.toasts.push({ ...toast, id: id });
    // todo move this into the toast component and add animation
    // this.setRemoveAfterDelay(id, toast.delay);
  }

  public showInfo(title: string, message: string, delay?: number): void {
    this.show({ title, message, delay });
  }

  public showSuccess(title: string, message: string, delay?: number): void {
    this.show({ title, message, classname: 'success', delay });
  }

  public showDanger(title: string, message: string): void {
    this.show({ title, message, classname: 'danger' });
  }

  public showWarning(title: string, message: string, delay?: number): void {
    this.show({ title, message, classname: 'warning', delay });
  }

  public remove(id: string): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
  }

  // private setRemoveAfterDelay(id: string, delay?: number): void {
  //   if (delay && delay > 0) {
  //     setTimeout(() => {
  //       this.remove(id);
  //     }, delay);
  //   }
  // }

}

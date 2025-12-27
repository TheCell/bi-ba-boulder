import { Injectable, Type } from '@angular/core';
import { Modal } from './modal/modal';
import { IModal } from './modal/modal.interface';
import { CloseModalEvent } from './modal/close-modal-event';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private modals: Modal[] = [];

  public add(modal: Modal): void {
    if (modal.id.length === 0 || this.modals.find(m => m.id === modal.id)) {
      throw new Error('modal already exists');
    }

    this.modals.push(modal);
  }

  public remove(id: string): void {
    this.modals = this.modals.filter(m => m.id !== id);
  }

  public open(id: string, component?: Type<IModal>): IModal | void {
    const modal = this.modals.find(m => m.id === id);

    if (!modal) {
      throw new Error('modal not found');
    }
    
    if (component) {
      return modal.open(component);
    } else {
      modal.openWithExternalContent();
    }
  }

  public closeAll(closeModalEvent: CloseModalEvent): void {
    this.modals.forEach(modal => modal.close(closeModalEvent));
  }

}

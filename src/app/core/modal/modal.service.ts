import { Injectable } from '@angular/core';
import { Modal } from './modal/modal';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private modals: Modal[] = [];

  constructor() { }

  public add(modal: Modal): void {
    if (modal.id.length === 0 || this.modals.find(m => m.id === modal.id)) {
      throw new Error('modal already exists');
    }

    this.modals.push(modal);
  }

  public remove(id: string): void {
    this.modals = this.modals.filter(m => m.id !== id);
  }

  public open(id: string): void {
    const modal = this.modals.find(m => m.id === id);

    if (!modal) {
      throw new Error('modal not found');
    }
    
    modal.open();
  }

  public close(): void {
    this.modals.forEach(modal => modal.close());
  }

}

import { Component, inject, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Modal } from './core/modal/modal/modal';
import { ModalService } from './core/modal/modal.service';
import { LoginDialogComponent } from './core/modal/login-dialog/login-dialog.component';
import { RegistrationDialogComponent } from './core/modal/registration-dialog/registration-dialog.component';
import { ToastContainer } from "./core/toast-container/toast-container";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Modal, ToastContainer],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  @ViewChild('modal') private modal!: Modal;
  private modalService = inject(ModalService);

  public title = 'bibaboulder';

  public constructor() {
    // this.toastService.show({ title: 'Info', message: 'Welcome', delay: 1000 });
    // this.toastService.showInfo('Info', 'Welcome to BiBa Boulder!');
    // this.toastService.showSuccess('Success', 'You have successfully logged in!');
    // this.toastService.showDanger('Error', 'Error: Unable to load user data.');
    // this.toastService.showWarning('Warning', 'Warning: Your password will expire in 7 days.');
    // this.toastService.show({ message: 'Welcome to BiBa Boulder!' });
    // this.toastService.show({ message: 'Welcome to BiBa asdfas dfasdf asdfas dfsdfa adf  Boulder!' });
    // this.toastService.show({ message: 'Welcome to BiBa asdf asdf asdf asdf asd fasdf asdf asdf asdf asdf asdf asdf asdf asdf asdf asdf Boulder!' });
  }

  public openLoginModal(): void {
    this.modalService.open(this.modal.id, LoginDialogComponent);
  }

  public openRegistrationModal(): void {
    this.modalService.open(this.modal.id, RegistrationDialogComponent);
  }
}
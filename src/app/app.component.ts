import { Component, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Modal } from './core/modal/modal/modal';
import { ModalService } from './core/modal/modal.service';
import { LoginDialogComponent } from './core/modal/login-dialog/login-dialog.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Modal, LoginDialogComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  @ViewChild('loginModal') private loginModal!: Modal;

  public title = 'bibaboulder';

  public constructor(private modalService: ModalService) {
  }

  public openLoginModal(): void {
    // this.modalService.add(new Modal());
    this.modalService.open(this.loginModal.id);
  }
}

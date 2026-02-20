import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { Modal } from './core/modal/modal/modal';
import { ModalService } from './core/modal/modal.service';
import { LoginDialogComponent } from './core/modal/login-dialog/login-dialog.component';
import { ToastContainer } from "./core/toast-container/toast-container";
import { LoginTrackerService } from './auth/login-tracker.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Modal, ToastContainer],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit, OnDestroy {
  @ViewChild('modal') private modal!: Modal;
  public loginTrackerService = inject(LoginTrackerService);
  public changeDetectorRef = inject(ChangeDetectorRef);
  private modalService = inject(ModalService);
  private subscription = new Subscription();

  public title = 'bibaboulder';

  public ngOnInit(): void {
    this.subscription.add(
      this.loginTrackerService.authStateChanged$.subscribe(() => {
        this.changeDetectorRef.markForCheck();
      })
    );
    this.loginTrackerService.checkSession();
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public openLoginModal(): void {
    this.modalService.open(this.modal.id, LoginDialogComponent);
  }

  public logout(): void {
    this.loginTrackerService.logout();
  }
}

import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { IModal } from '../modal/modal.interface';
import { LoginTrackerService } from 'src/app/auth/login-tracker.service';
import { CloseModalEvent } from '../modal/close-modal-event';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-login-dialog',
  imports: [],
  templateUrl: './login-dialog.component.html',
  styleUrl: './login-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.Default
})
export class LoginDialogComponent implements IModal {
  private loginTrackerService = inject(LoginTrackerService);

  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public isDevelopment = !environment.production;

  public onLogin(): void {
    this.loginTrackerService.login();
  }

  public onDevLogin(): void {
    this.loginTrackerService.devLogin();
    this.closeModal.emit({ closeType: 0 });
  }
}

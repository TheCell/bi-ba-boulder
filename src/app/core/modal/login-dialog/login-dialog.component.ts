import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { IModal } from '../modal/modal.interface';
import { LoginTrackerService } from 'src/app/auth/login-tracker.service';
import { CloseModalEvent } from '../modal/close-modal-event';

@Component({
  selector: 'app-login-dialog',
  imports: [],
  templateUrl: './login-dialog.component.html',
  styleUrl: './login-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class LoginDialogComponent implements IModal {
  private loginTrackerService = inject(LoginTrackerService);

  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;

  public onLogin(): void {
    this.loginTrackerService.login();
  }
}

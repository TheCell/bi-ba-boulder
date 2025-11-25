import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-login-dialog',
  imports: [CommonModule],
  templateUrl: './login-dialog.component.html',
  styleUrl: './login-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginDialogComponent {
  
}

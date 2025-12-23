import { ChangeDetectionStrategy, Component } from '@angular/core';
import { iModal } from 'src/app/core/modal/modal/modal.interface';

@Component({
  selector: 'app-spraywall-info-dialog',
  imports: [],
  templateUrl: './spraywall-info-dialog.html',
  styleUrl: './spraywall-info-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallInfoDialog implements iModal {
  public canCloseWithoutPermission = true;
}

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { iModal } from 'src/app/core/modal/modal/modal.interface';
import { SpraywallGradeFilterDialogData } from './spraywall-grade-filter-dialog-data';

@Component({
  selector: 'app-spraywall-grade-filter',
  imports: [],
  templateUrl: './spraywall-grade-filter.html',
  styleUrl: './spraywall-grade-filter.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallGradeFilter implements iModal {
  public canCloseWithoutPermission = true;

  public initialize(data: SpraywallGradeFilterDialogData): void {
    console.log(data);
  }

  public onClose(): void {
    console.log('onClose');
  }

}

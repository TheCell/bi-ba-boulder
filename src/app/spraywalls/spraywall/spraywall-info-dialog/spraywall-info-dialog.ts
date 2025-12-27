import { ChangeDetectionStrategy, Component, output } from '@angular/core';
import { CloseModalEvent } from 'src/app/core/modal/modal/close-modal-event';
import { IModal } from 'src/app/core/modal/modal/modal.interface';
import { holdColorOptions, SpraywallHoldType, TypeAndColor } from 'src/app/renderer/common/spraywall-hold-types';

@Component({
  selector: 'app-spraywall-info-dialog',
  imports: [],
  templateUrl: './spraywall-info-dialog.html',
  styleUrl: './spraywall-info-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallInfoDialog implements IModal {
  public closeModal = output<CloseModalEvent>();

  public canCloseWithoutPermission = true;
  public holdColorOptions: TypeAndColor[] = holdColorOptions;
  public colorFormId = ''.appendUniqueId();


  public enumName(type: SpraywallHoldType): string {
    const enumNames = Object.keys(SpraywallHoldType).filter(key => isNaN(Number(key)));
    return enumNames[type];
  }

  public onClose(): void {
    this.closeModal.emit({ closeType: 0 });
  }
}

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ModalService } from 'src/app/core/modal/modal.service';
import { iModal } from 'src/app/core/modal/modal/modal.interface';
import { holdColorOptions, SpraywallHoldType, TypeAndColor } from 'src/app/renderer/common/spraywall-hold-types';

@Component({
  selector: 'app-spraywall-info-dialog',
  imports: [],
  templateUrl: './spraywall-info-dialog.html',
  styleUrl: './spraywall-info-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallInfoDialog implements iModal {
  public canCloseWithoutPermission = true;
  public holdColorOptions: TypeAndColor[] = holdColorOptions;
  public colorFormId = ''.appendUniqueId();

  private modalService = inject(ModalService);

  public enumName(type: SpraywallHoldType): string {
    const enumNames = Object.keys(SpraywallHoldType).filter(key => isNaN(Number(key)));
    return enumNames[type];
  }

  public onClose(): void {
    this.modalService.close({ closeType: 0 });
  }
}

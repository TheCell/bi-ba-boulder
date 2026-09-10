import { Component, output } from '@angular/core';
import { BoulderGymDto } from '@api-net/index';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { BoulderGymDialogData } from './boulder-gym-dialog-data';

@Component({
  selector: 'app-delete-boulder-gym-dialog',
  templateUrl: './delete-boulder-gym-dialog.html'
})
export class DeleteBoulderGymDialog implements IModal {
  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public boulderGym: BoulderGymDto = null!;

  public initialize(data: BoulderGymDialogData): void {
    this.boulderGym = data.boulderGym!;
  }
}

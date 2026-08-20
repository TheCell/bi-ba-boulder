import { Component, output } from '@angular/core';
import { IModal } from '../modal/modal.interface';
import { CloseModalEvent } from '../modal/close-modal-event';

@Component({
  selector: 'app-image-holder-modal',
  imports: [],
  templateUrl: './image-holder-modal.html',
  styleUrl: './image-holder-modal.scss'
})
export class ImageHolderModal implements IModal {
  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public imageUri = '';

  public initialize({ imageUri }: { imageUri: string }): void {
    this.imageUri = imageUri;
  }
}


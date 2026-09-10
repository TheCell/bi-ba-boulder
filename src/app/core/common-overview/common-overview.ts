import { Component, computed, inject, input, ViewChild } from '@angular/core';
import { Modal } from '../modal/modal/modal';
import { ModalService } from '../modal/modal.service';
import { ImageHolderModal } from '../modal/image-holder-modal/image-holder-modal';
import { PublicResourceDto } from '@api-net/index';
import { ResourceType } from '../enums/resource-type.enum';

@Component({
  selector: 'app-common-overview',
  imports: [Modal],
  templateUrl: './common-overview.html',
  styleUrl: './common-overview.scss'
})
export class CommonOverview {
  @ViewChild('imageModal') private imageModal!: Modal;
  private modalService = inject(ModalService);

  public name = input<string>('');
  public media = input<PublicResourceDto[]>([]);
  public importantInfo = input<string | undefined>(undefined);
  public description = input<string | undefined>(undefined);
  public imageUris = computed(() => {
    return this.media()
      .filter((resource: PublicResourceDto): boolean => resource.resourceType === ResourceType.Image)
      .map((resource: PublicResourceDto): string => resource.uri);
  });

  public openImageModal(uri: string): void {
    const imageHolderModal = this.modalService.open(this.imageModal.id, ImageHolderModal);
    if (imageHolderModal && imageHolderModal.initialize) {
      imageHolderModal.initialize({ imageUri: uri });
    }
  }
}


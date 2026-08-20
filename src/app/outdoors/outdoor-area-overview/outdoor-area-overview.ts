import { Component, inject, ViewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OutdoorAreaDto, PublicResourceDto } from '@api-net/index';
import { ResourceType } from '../../core/enums/resource-type.enum';
import { Modal } from '../../core/modal/modal/modal';
import { ModalService } from '../../core/modal/modal.service';
import { ImageHolderModal } from '../../core/modal/image-holder-modal/image-holder-modal';

@Component({
  selector: 'app-outdoor-area-overview',
  imports: [RouterLink, Modal],
  templateUrl: './outdoor-area-overview.html',
  styleUrl: './outdoor-area-overview.scss'
})
export class OutdoorAreaOverview {
  @ViewChild('imageModal') private imageModal!: Modal;

  private modalService = inject(ModalService);

  public readonly outdoorArea: OutdoorAreaDto;
  public readonly imageUris: readonly string[];

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.outdoorArea = activatedRoute.snapshot.data['outdoorArea'];
    this.imageUris = this.getImageUris(this.outdoorArea);
  }

  public openImageModal(uri: string): void {
    const imageHolderModal = this.modalService.open(this.imageModal.id, ImageHolderModal);
    if (imageHolderModal && imageHolderModal.initialize) {
      imageHolderModal.initialize({ imageUri: uri });
    }
  }

  private getImageUris(outdoorArea: OutdoorAreaDto): readonly string[] {
    const imageUris: string[] = (outdoorArea.images ?? [])
      .filter((resource: PublicResourceDto): boolean => resource.resourceType !== ResourceType.Image)
      .map((resource: PublicResourceDto): string => resource.uri);

    if (imageUris.length === 0 && outdoorArea.previewImageUri) {
      return [outdoorArea.previewImageUri];
    }

    return imageUris;
  }
}


import { ChangeDetectionStrategy, Component, inject, ViewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BlocDto, PublicResourceDto, SectorDto } from '@api-net/index';
import { ResourceType } from '../../core/enums/resource-type.enum';
import { Modal } from '../../core/modal/modal/modal';
import { ModalService } from '../../core/modal/modal.service';
import { ImageHolderModal } from '../../core/modal/image-holder-modal/image-holder-modal';

@Component({
  selector: 'app-sector',
  imports: [RouterLink, Modal],
  templateUrl: './sector.component.html',
  styleUrl: './sector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SectorComponent {
  @ViewChild('imageModal') private imageModal!: Modal;

  private modalService = inject(ModalService);
  private activatedRoute = inject(ActivatedRoute);
  public sector: SectorDto;
  public blocs: BlocDto[] = [];
  public readonly imageUris: readonly string[];
  public readonly outdoorAreaId: string | null;

  public constructor() {
    this.blocs = this.activatedRoute.snapshot.data['blocs'];
    this.sector = this.activatedRoute.snapshot.data['sector'];
    this.imageUris = this.getImageUris(this.sector);
    this.outdoorAreaId = this.activatedRoute.snapshot.paramMap.get('outdoorAreaId');
  }

  public openImageModal(uri: string): void {
    const imageHolderModal = this.modalService.open(this.imageModal.id, ImageHolderModal);
    if (imageHolderModal && imageHolderModal.initialize) {
      imageHolderModal.initialize({ imageUri: uri });
    }
  }

  private getImageUris(sector: SectorDto): readonly string[] {
    const imageUris: string[] = (sector.images ?? [])
      .filter((resource: PublicResourceDto): boolean => resource.resourceType !== ResourceType.Image)
      .map((resource: PublicResourceDto): string => resource.uri);

    if (imageUris.length === 0 && sector.previewImageUri) {
      return [sector.previewImageUri];
    }

    return imageUris;
  }
}

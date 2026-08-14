import { NgOptimizedImage } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OutdoorAreaDto, PublicResourceDto } from '@api-net/index';
import { ResourceType } from '../../core/enums/resource-type.enum';

@Component({
  selector: 'app-outdoor-area-overview',
  imports: [NgOptimizedImage, RouterLink],
  templateUrl: './outdoor-area-overview.html',
  styleUrl: './outdoor-area-overview.scss'
})
export class OutdoorAreaOverview {
  public readonly outdoorArea: OutdoorAreaDto;
  public readonly imageUris: readonly string[];

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.outdoorArea = activatedRoute.snapshot.data['outdoorArea'];
    this.imageUris = this.getImageUris(this.outdoorArea);
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


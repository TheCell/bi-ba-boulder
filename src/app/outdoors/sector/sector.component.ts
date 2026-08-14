import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BlocDto, PublicResourceDto, SectorDto } from '@api-net/index';
import { ResourceType } from '../../core/enums/resource-type.enum';
import { NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-sector',
  imports: [RouterLink, NgOptimizedImage],
  templateUrl: './sector.component.html',
  styleUrl: './sector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SectorComponent {
  private activatedRoute = inject(ActivatedRoute);
  public sector: SectorDto;
  public blocs: BlocDto[] = [];
  public readonly imageUris: readonly string[];

  public constructor() {
    this.blocs = this.activatedRoute.snapshot.data['blocs'];
    this.sector = this.activatedRoute.snapshot.data['sector'];
    this.imageUris = this.getImageUris(this.sector);
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

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BlocDto, SectorDto } from '@api-net/index';
import { CommonOverview } from '../../core/common-overview/common-overview';

@Component({
  selector: 'app-sector',
  imports: [RouterLink, CommonOverview],
  templateUrl: './sector.component.html',
  styleUrl: './sector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SectorComponent {
  private activatedRoute = inject(ActivatedRoute);
  public sector: SectorDto;
  public blocs: BlocDto[] = [];
  public readonly outdoorAreaId: string | null;

  public constructor() {
    this.blocs = this.activatedRoute.snapshot.data['blocs'];
    this.sector = this.activatedRoute.snapshot.data['sector'];
    this.outdoorAreaId = this.activatedRoute.snapshot.paramMap.get('outdoorAreaId');
  }
}

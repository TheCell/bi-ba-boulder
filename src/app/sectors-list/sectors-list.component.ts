import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SectorDto } from '@api-net/index';

@Component({
  selector: 'app-sectors-list',
  imports: [RouterLink],
  templateUrl: './sectors-list.component.html',
  styleUrl: './sectors-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SectorsListComponent {
  private activatedRoute = inject(ActivatedRoute);
  public sectors: SectorDto[] = [];

  public constructor() {
    const sectors = this.activatedRoute.snapshot.data['sectors'];
    this.sectors = sectors;
  }
}

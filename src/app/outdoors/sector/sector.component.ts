import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BlocDto } from '@api-net/index';

@Component({
  selector: 'app-sector',
  imports: [RouterLink],
  templateUrl: './sector.component.html',
  styleUrl: './sector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SectorComponent {
  private activatedRoute = inject(ActivatedRoute);
  public blocs: BlocDto[] = [];

  public constructor() {
    this.blocs = this.activatedRoute.snapshot.data['blocs'];
  }
}

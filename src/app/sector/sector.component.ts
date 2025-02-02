import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BlocDto } from '../api';

@Component({
    selector: 'app-sector',
    imports: [
        CommonModule,
        RouterLink
    ],
    templateUrl: './sector.component.html',
    styleUrl: './sector.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SectorComponent {
  public blocs: BlocDto[] = [];

  public constructor(activatedRoute: ActivatedRoute) {
    this.blocs = activatedRoute.snapshot.data['blocs'];
  }

}

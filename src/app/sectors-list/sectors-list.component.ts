import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SectorDto } from '../api';

@Component({
    selector: 'app-sectors-list',
    imports: [
        CommonModule,
        RouterLink
    ],
    templateUrl: './sectors-list.component.html',
    styleUrl: './sectors-list.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SectorsListComponent {
  public sectors: SectorDto[] = [];

  public constructor(activatedRoute: ActivatedRoute) {
    const sectors = activatedRoute.snapshot.data['sectors'];
    this.sectors = sectors;
  }
}

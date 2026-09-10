import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OutdoorAreaDto } from '@api-net/index';
import { CommonOverview } from '../../core/common-overview/common-overview';

@Component({
  selector: 'app-outdoor-area-overview',
  imports: [RouterLink, CommonOverview],
  templateUrl: './outdoor-area-overview.html',
  styleUrl: './outdoor-area-overview.scss'
})
export class OutdoorAreaOverview {
  public readonly outdoorArea: OutdoorAreaDto;

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.outdoorArea = activatedRoute.snapshot.data['outdoorArea'];
  }
}


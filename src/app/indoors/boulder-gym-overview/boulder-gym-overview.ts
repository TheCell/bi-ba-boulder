import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BoulderGymDto } from '@api-net/index';
import { CommonOverview } from '../../core/common-overview/common-overview';

@Component({
  selector: 'app-boulder-gym-overview',
  imports: [RouterLink, CommonOverview],
  templateUrl: './boulder-gym-overview.html',
  styleUrl: './boulder-gym-overview.scss'
})
export class BoulderGymOverview {
  public readonly boulderGym: BoulderGymDto;

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.boulderGym = activatedRoute.snapshot.data['boulderGym'];
  }
}


import { ChangeDetectorRef, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FeedbackOverlay } from '../core/feedback-overlay/feedback-overlay';
import {
  OutdoorAreasService,
  SpraywallsService,
  OutdoorAreaDto,
  SpraywallDto,
  BoulderGymDto,
  BoulderGymService
} from '@api-net/index';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-home',
  imports: [RouterLink, FeedbackOverlay, NgClass],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  private spraywallsService = inject(SpraywallsService);
  private outdoorAreasService = inject(OutdoorAreasService);
  private boulderGymsService = inject(BoulderGymService);
  private changeDetectorRef = inject(ChangeDetectorRef);

  public readonly patternTiles: readonly number[] = Array.from({ length: 12 }, (_, index: number) => index);
  public spraywalls = signal<SpraywallDto[]>([]);
  public boulderGyms = signal<BoulderGymDto[]>([]);
  public outdoorAreas = signal<OutdoorAreaDto[]>([]);

  constructor() {
    this.spraywallsService.getSpraywalls().subscribe({
      next: (spraywalls) => {
        this.spraywalls.set(spraywalls);
        this.changeDetectorRef.markForCheck();
      }
    });

    this.outdoorAreasService.getOutdoorAreas().subscribe({
      next: (outdoorAreas) => {
        this.outdoorAreas.set(outdoorAreas);
        this.changeDetectorRef.markForCheck();
      }
    });

    this.boulderGymsService.getBoulderGyms().subscribe({
      next: (boulderGyms) => {
        this.boulderGyms.set(boulderGyms);
        this.changeDetectorRef.markForCheck();
      }
    });
  }
}

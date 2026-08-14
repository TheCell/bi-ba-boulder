import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FeedbackOverlay } from '../core/feedback-overlay/feedback-overlay';
import { OutdoorAreasService, SpraywallsService, OutdoorAreaDto, SpraywallDto } from '@api-net/index';

@Component({
  selector: 'app-home',
  imports: [RouterLink, FeedbackOverlay],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  private spraywallsService = inject(SpraywallsService);
  private outdoorAreasService = inject(OutdoorAreasService);

  public readonly patternTiles: readonly number[] = Array.from({ length: 12 }, (_, index: number) => index);
  public spraywalls = signal<SpraywallDto[]>([]);
  public outdoorAreas = signal<OutdoorAreaDto[]>([]);

  constructor() {
    this.spraywallsService.getSpraywalls().subscribe({
      next: (spraywalls) => {
        this.spraywalls.set(spraywalls);
      }
    });

    this.outdoorAreasService.getOutdoorAreas().subscribe({
      next: (outdoorAreas) => {
        this.outdoorAreas.set(outdoorAreas);
      }
    });
  }
}

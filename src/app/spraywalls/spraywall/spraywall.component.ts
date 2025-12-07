import { Component, inject, OnInit } from '@angular/core';
import { BoulderRenderComponent } from '../../renderer/boulder-render/boulder-render.component';

import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { SpraywallProblemDto, SpraywallsService } from '@api/index';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { SpraywallLegendItem } from './spraywall-legend-item/spraywall-legend-item';

@Component({
  selector: 'app-spraywall',
  imports: [LoadingImageComponent, BoulderRenderComponent, SpraywallLegendItem, RouterLink],
  templateUrl: './spraywall.component.html',
  styleUrl: './spraywall.component.scss',
})
export class SpraywallComponent implements OnInit {
  private spraywallsService = inject(SpraywallsService);
  private boulderLoaderService = inject(BoulderLoaderService)

  public currentRawModel?: ArrayBuffer;
  public spraywallId = '';

  public listOfProblems: SpraywallProblemDto[] = [];
  public selectedProblem?: SpraywallProblemDto = undefined;

  public constructor() {
    const route = inject(ActivatedRoute);
    this.spraywallId = route.snapshot.params['id'];
  }

  public ngOnInit() {
    this.boulderLoaderService.loadTestSpraywall3().subscribe({
      next: (data: ArrayBuffer) => {
        this.currentRawModel = data;
      }
    });

    this.spraywallsService.getProblems(this.spraywallId).subscribe({
      next: (problems: SpraywallProblemDto[]) => {
        this.listOfProblems = problems;
      }
    });
  }
  
  public onSelectedProblem(problem: SpraywallProblemDto): void {
    this.selectedProblem = problem;
  }

  public onResetSelection(): void {
    this.selectedProblem = undefined;
  }
}
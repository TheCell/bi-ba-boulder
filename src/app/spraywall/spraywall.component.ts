import { Component, inject, OnInit } from '@angular/core';
import { BoulderRenderComponent } from '../renderer/boulder-render/boulder-render.component';
import { CommonModule } from '@angular/common';
import { LoadingImageComponent } from '../common/loading-image/loading-image.component';
import { DefaultService, SpraywallProblemDto } from '../api';
import { ActivatedRoute } from '@angular/router';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import { SpraywallLegendItem } from './spraywall-legend-item/spraywall-legend-item';

@Component({
  selector: 'app-spraywall',
  imports: [CommonModule, LoadingImageComponent, BoulderRenderComponent, SpraywallLegendItem],
  templateUrl: './spraywall.component.html',
  styleUrl: './spraywall.component.scss',
})
export class SpraywallComponent implements OnInit {
  private defaultService = inject(DefaultService);
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

    this.defaultService.getSpraywallProblems(this.spraywallId).subscribe({
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
import { Component, inject, OnInit } from '@angular/core';
import { BoulderRenderComponent } from '../boulder-render/boulder-render.component';
import { CommonModule } from '@angular/common';
import { LoadingImageComponent } from '../common/loading-image/loading-image.component';
import { DefaultService, SpraywallProblemDto } from '../api';
import { ActivatedRoute } from '@angular/router';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';

@Component({
  selector: 'app-spraywall',
  imports: [CommonModule, LoadingImageComponent, BoulderRenderComponent],
  templateUrl: './spraywall.component.html',
  styleUrl: './spraywall.component.scss',
})
export class SpraywallComponent implements OnInit {
  private defaultService = inject(DefaultService);
  private boulderLoaderService = inject(BoulderLoaderService)

  public currentRawModel?: ArrayBuffer;
  public spraywallId = '';

  public constructor() {
    const route = inject(ActivatedRoute);
    this.spraywallId = route.snapshot.params['id'];
  }

  public ngOnInit() {
    this.boulderLoaderService.loadTestSpraywall2().subscribe({
      next: (data: ArrayBuffer) => {
        this.currentRawModel = data;
      }
    });

    this.defaultService.getSpraywallProblems(this.spraywallId).subscribe({
      next: (problems: SpraywallProblemDto[]) => {
        console.log(problems);
        
      }
    });
  }
}

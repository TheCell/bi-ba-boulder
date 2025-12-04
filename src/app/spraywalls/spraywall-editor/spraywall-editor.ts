import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { BoulderRenderComponent } from 'src/app/renderer/boulder-render/boulder-render.component';
import { LoadingImageComponent } from 'src/app/common/loading-image/loading-image.component';
import { SpraywallProblemDto, SpraywallService } from '@api/index';
import { BoulderLoaderService } from 'src/app/background-loading/boulder-loader.service';

@Component({
  selector: 'app-spraywall-editor',
  imports: [LoadingImageComponent, BoulderRenderComponent],
  templateUrl: './spraywall-editor.html',
  styleUrl: './spraywall-editor.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallEditor implements OnInit {
  private spraywallService = inject(SpraywallService);
  private boulderLoaderService = inject(BoulderLoaderService)

  public currentRawModel?: ArrayBuffer;
  public spraywallId = '';
  public selectedProblem?: SpraywallProblemDto = undefined;

  public ngOnInit() {
    console.log('temp');
    
    // this.boulderLoaderService.loadTestSpraywall3().subscribe({
    //   next: (data: ArrayBuffer) => {
    //     this.currentRawModel = data;
    //   }
    // });

    // this.spraywallService.getProblems(this.spraywallId).subscribe({
    //   next: (problems: SpraywallProblemDto[]) => {
    //     this.listOfProblems = problems;
    //   }
    // });
  }
  
  // public onSelectedProblem(problem: SpraywallProblemDto): void {
  //   this.selectedProblem = problem;
  // }

  public onResetSelection(): void {
    this.selectedProblem = undefined;
  }
}

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import { BoulderLine } from '../interfaces/boulder-line';
import { BoulderDebugRenderComponent } from '../boulder-debug-render/boulder-debug-render.component';
import { DefaultService, SpraywallProblemDto } from '../api';

@Component({
  selector: 'app-spraywall-test-2',
  imports: [BoulderDebugRenderComponent],
  templateUrl: './spraywall-test-2.component.html',
  styleUrl: './spraywall-test-2.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SpraywallTest2Component implements OnInit {
  private defaultService = inject(DefaultService);
  private changeDetectorRef = inject(ChangeDetectorRef);

  public currentRawModel?: ArrayBuffer = undefined;
  public currentLines: BoulderLine[] = [];
  public selectedProblemId?: string;
  public spraywallProblems: SpraywallProblemDto[] = [];
  
  public constructor(
    private boulderLoaderService: BoulderLoaderService) {
    const testBoulder = this.boulderLoaderService.loadTestSpraywall2();
    testBoulder.subscribe({
      next: (data) => {
        // console.log('data', data);
        this.currentRawModel = data;
        this.changeDetectorRef.markForCheck();
        // this.addBoulderToScene(data);
      }
    });
  }

  public ngOnInit(): void {
    this.defaultService.getSpraywallProblems("1").subscribe({
      next: (spraywallProblems: SpraywallProblemDto[]) => {
        this.spraywallProblems = spraywallProblems;
        this.changeDetectorRef.markForCheck();
      }
    });
  }

  public selectProblem(problemId: string): void {
    this.selectedProblemId = problemId;
  }
}

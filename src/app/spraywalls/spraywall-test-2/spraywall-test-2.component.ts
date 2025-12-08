import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { BoulderLine } from '../../interfaces/boulder-line';
import { BoulderDebugRenderComponent } from '../../renderer/boulder-debug-render/boulder-debug-render.component';
import { SpraywallProblemDto, SpraywallsService } from '@api/index';

@Component({
  selector: 'app-spraywall-test-2',
  imports: [BoulderDebugRenderComponent],
  templateUrl: './spraywall-test-2.component.html',
  styleUrl: './spraywall-test-2.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SpraywallTest2Component implements OnInit {
  private spraywallsService: SpraywallsService = inject(SpraywallsService);
  private changeDetectorRef: ChangeDetectorRef = inject(ChangeDetectorRef);
  private boulderLoaderService: BoulderLoaderService = inject(BoulderLoaderService);

  public currentRawModel?: ArrayBuffer = undefined;
  public currentLines: BoulderLine[] = [];
  public selectedProblemId?: string;
  public spraywallProblems: SpraywallProblemDto[] = [];
  
  public constructor() {
    const testBoulder = this.boulderLoaderService.loadTestSpraywall3();
    testBoulder.subscribe({
      next: (data) => {
        this.currentRawModel = data;
        this.changeDetectorRef.markForCheck();
      }
    });
  }

  public ngOnInit(): void {
    this.spraywallsService.getProblems('e4b5991c-54c8-f011-9457-71a4df1b7093').subscribe({
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

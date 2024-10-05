import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';
import { DaoneRenderTestComponent } from '../daone-render-test/daone-render-test.component';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import { BoulderProblemsService } from '../background-loading/boulder-problems.service';
import { Subject, Subscription } from 'rxjs';
import { ResolutionLevel } from '../api/interfaces/resolution-level';
import { BoulderLine } from '../api/interfaces/boulder-line';

@Component({
  selector: 'app-daone-test',
  standalone: true,
  imports: [
    CommonModule,
    DaoneRenderTestComponent
  ],
  templateUrl: './daone-test.component.html',
  styleUrl: './daone-test.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DaoneTestComponent implements OnDestroy {
  public title: string;
  public boulderImageUrl: SafeStyle;
  public image: string;
  public modelLoaded = false;

  public currentRawModel?: ArrayBuffer;
  public currentLines: BoulderLine[] = [];

  private finishedLoading = new Subject<ResolutionLevel>();
  private subscription = new Subscription();

  public constructor(
    private boulderLoaderService: BoulderLoaderService,
    private boulderProblemsService: BoulderProblemsService,
    private domSanitizer: DomSanitizer,
    private changeDetectorRef: ChangeDetectorRef) {
    this.title = 'test';
    this.image = './test-images/Bloc_5.jpg';
    this.boulderImageUrl = this.domSanitizer.bypassSecurityTrustStyle('url(./test-images/Bloc_5.jpg)');

    setTimeout(() => {
      // faking for now
      this.modelLoaded = true;
    }, 2000);

    const testBoulder = this.boulderLoaderService.loadTestDaoneBoulder('low');
    testBoulder.subscribe({
      next: (data: ArrayBuffer) => {
        this.currentRawModel = data;
        this.finishedLoading.next('low');
      }
    });

    this.subscription.add(this.finishedLoading.subscribe({
      next: (resolutionLevel: ResolutionLevel) => {
        this.changeDetectorRef.markForCheck();

        if (resolutionLevel === 'low') {
          this.boulderLoaderService.loadTestDaoneBoulder('medium').subscribe({
            next: (data) => {
              this.currentRawModel = data;
              this.finishedLoading.next('medium');
            }
          });
        } else if (resolutionLevel === 'medium') {
          this.boulderLoaderService.loadTestDaoneBoulder('high').subscribe({
            next: (data) => {
              this.currentRawModel = data;
              this.finishedLoading.next('high');
            }
          });
        }
      }
    }));

    this.subscription.add(this.boulderProblemsService.loadDaoneTestBoulderProblem().subscribe({
      next: (data: BoulderLine[]) => {
        this.currentLines = data;
        this.changeDetectorRef.markForCheck();
      }
    }));
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}

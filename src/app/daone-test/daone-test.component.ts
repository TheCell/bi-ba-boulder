import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';
import { DaoneRenderTestComponent } from '../daone-render-test/daone-render-test.component';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import { BoulderProblemsService } from '../background-loading/boulder-problems.service';
import { Subject, Subscription } from 'rxjs';
import { BoulderLegendComponent } from '../components/boulder-legend/boulder-legend.component';
import { ActivatedRoute } from '@angular/router';
import { DefaultService } from '../api';
import { BoulderLine } from '../interfaces/boulder-line';
import { ResolutionLevel } from '../interfaces/resolution-level';
import { HttpClient } from '@angular/common/http';

@Component({
    selector: 'app-daone-test',
    imports: [
        CommonModule,
        DaoneRenderTestComponent,
        BoulderLegendComponent
    ],
    templateUrl: './daone-test.component.html',
    styleUrl: './daone-test.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
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
  private number: string | null;

  public constructor(
    private boulderLoaderService: BoulderLoaderService,
    private boulderProblemsService: BoulderProblemsService,
    private defaultService: DefaultService,
    private domSanitizer: DomSanitizer,
    private changeDetectorRef: ChangeDetectorRef,
    private activatedRoute: ActivatedRoute,
    private httpClient: HttpClient) {
    this.title = 'test';
    this.image = './test-images/Bloc_5.jpg';
    this.boulderImageUrl = this.domSanitizer.bypassSecurityTrustStyle('url(./test-images/Bloc_5.jpg)');
    this.number = this.activatedRoute.snapshot.paramMap.get('number');

    setTimeout(() => {
      // faking for now
      this.modelLoaded = true;
    }, 2000);

    if (this.number) {
      const testBoulder = this.boulderLoaderService.loadTestDaoneBoulder2('low');
      testBoulder.subscribe({
        next: (data: ArrayBuffer) => {
          this.currentRawModel = data;
          this.finishedLoading.next('low');
        }
      });
    } else {
      const testBoulder = this.boulderLoaderService.loadTestDaoneBoulder('low');
      testBoulder.subscribe({
        next: (data: ArrayBuffer) => {
          this.currentRawModel = data;
          this.finishedLoading.next('low');
        }
      });
    }

    this.subscription.add(this.finishedLoading.subscribe({
      next: (resolutionLevel: ResolutionLevel) => {
        this.changeDetectorRef.markForCheck();

        if (resolutionLevel === 'low') {
          if (this.number) {
            this.boulderLoaderService.loadTestDaoneBoulder2('medium').subscribe({
              next: (data) => {
                this.currentRawModel = data;
                this.finishedLoading.next('medium');
              }
            });
          } else {
            this.boulderLoaderService.loadTestDaoneBoulder('medium').subscribe({
              next: (data) => {
                this.currentRawModel = data;
                this.finishedLoading.next('medium');
              }
            });
          }
        } else if (resolutionLevel === 'medium') {
          if (this.number) {
            this.boulderLoaderService.loadTestDaoneBoulder2('high').subscribe({
              next: (data) => {
                this.currentRawModel = data;
                this.finishedLoading.next('high');
              }
            });
          } else {
            this.boulderLoaderService.loadTestDaoneBoulder('high').subscribe({
              next: (data) => {
                this.currentRawModel = data;
                this.finishedLoading.next('high');
              }
            });
          }
        }
      }
    }));

    if (this.number === null) {
      this.subscription.add(this.boulderProblemsService.loadDaoneTestBoulderProblem().subscribe({
        next: (data: BoulderLine[]) => {
          this.currentLines = data;
          this.changeDetectorRef.markForCheck();
        }
      }));
    }
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}

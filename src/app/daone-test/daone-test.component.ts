import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';
import { DaoneRenderTestComponent } from '../daone-render-test/daone-render-test.component';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import { BoulderProblemsService } from '../background-loading/boulder-problems.service';
import { interval, Subject, Subscription, timeInterval } from 'rxjs';
import { ResolutionLevel } from '../api/interfaces/resolution-level';
import { AsyncScheduler } from 'rxjs/internal/scheduler/AsyncScheduler';

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
  public testbool = false;

  private finishedLoading = new Subject<ResolutionLevel>();
  private subscription = new Subscription();
  private testTimer = interval(200);

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

    this.subscription.add(this.testTimer.subscribe({
      next: () => {
        this.testbool = !this.testbool;
        this.changeDetectorRef.markForCheck();
      }
    }));

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

    // const testRoutes = this.boulderProblemsService.loadDaoneTestBoulderProblem()
    // testRoutes.subscribe({
    //   next: (data: Array<BoulderLine>) => {
    //     data.forEach((boulderLine: BoulderLine) => {
    //       this.addLineToScene(this.scene, boulderLine.points.map((point) => new THREE.Vector3(point.x, point.y, point.z)), boulderLine.color)
    //     });
    //   }
    // });
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}

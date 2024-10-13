import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy } from '@angular/core';
import { BoulderLegendComponent } from '../components/boulder-legend/boulder-legend.component';
import { BoulderRenderComponent } from '../boulder-render/boulder-render.component';
import { ActivatedRoute } from '@angular/router';
import { Subject, Subscription, switchMap } from 'rxjs';
import { BlocDto } from '../api';
import { BoulderLine } from '../interfaces/boulder-line';
import { ResolutionLevel } from '../interfaces/resolution-level';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';

@Component({
  selector: 'app-boulder',
  standalone: true,
  imports: [
    CommonModule,
    BoulderRenderComponent,
    BoulderLegendComponent
  ],
  templateUrl: './boulder.component.html',
  styleUrl: './boulder.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoulderComponent implements OnDestroy {
  public bloc: BlocDto;

  public currentRawModel?: ArrayBuffer = undefined;
  public currentLines: BoulderLine[] = [];

  private loadNextResolution = new Subject<void>();
  private startLoadingBoulder = new Subject<void>();
  private subscription = new Subscription();
  private blocId: string | null;
  private boulderUrl = '';
  private resolutionToLoad?: ResolutionLevel;

  private boulderLoaderService = inject(BoulderLoaderService);

  public constructor(
    private activatedRoute: ActivatedRoute,
    private changeDetectorRef: ChangeDetectorRef) {
      this.bloc = this.activatedRoute.snapshot.data['bloc'];
      this.blocId = this.activatedRoute.snapshot.paramMap.get('id');

      this.subscription.add(this.loadNextResolution.subscribe({
        next: () => {
          if (this.resolutionToLoad !== undefined) {
            const urlAndInfo = this.boulderLoaderService.getUrl(this.bloc, this.resolutionToLoad);
            this.resolutionToLoad = urlAndInfo.higherResolution;
            this.boulderUrl = urlAndInfo.url;

            if (this.boulderUrl.length > 0) {
              this.startLoadingBoulder.next();
            }
          }
        }
      }));

      this.subscription.add(this.startLoadingBoulder.pipe(switchMap(() => this.boulderLoaderService.loadBoulder(this.boulderUrl))).subscribe({
        next: (data: ArrayBuffer) => {
          this.currentRawModel = data;
          this.loadNextResolution.next();
          this.changeDetectorRef.markForCheck();
        },
      }));

      const urlAndInfo = this.boulderLoaderService.getUrl(this.bloc);
      this.resolutionToLoad = urlAndInfo.higherResolution;
      this.boulderUrl = urlAndInfo.url;
      this.startLoadingBoulder.next();
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}

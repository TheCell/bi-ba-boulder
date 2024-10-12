import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy } from '@angular/core';
import { BoulderLegendComponent } from '../components/boulder-legend/boulder-legend.component';
import { BoulderRenderComponent } from '../boulder-render/boulder-render.component';
import { SafeStyle, DomSanitizer } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { BlocDto, DefaultService } from '../api';
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
  public boulderImageUrl: SafeStyle;
  public image: string;
  public bloc: BlocDto;

  public currentRawModel?: ArrayBuffer = undefined;
  public currentLines: BoulderLine[] = [];

  private finishedLoading = new Subject<void>();
  private subscription = new Subscription();
  private blocId: string | null;
  private boulderUrl: string;
  private nextResolutionToLoad?: ResolutionLevel;

  private defaultService = inject(DefaultService);
  private boulderLoaderService = inject(BoulderLoaderService);

  public constructor(
    private domSanitizer: DomSanitizer,
    private activatedRoute: ActivatedRoute) {
      this.bloc = this.activatedRoute.snapshot.data['bloc'];
      this.blocId = this.activatedRoute.snapshot.paramMap.get('id');
      this.image = './test-images/Bloc_5.jpg';
      this.boulderImageUrl = this.domSanitizer.bypassSecurityTrustStyle('url(./test-images/Bloc_5.jpg)');

      this.subscription.add(this.finishedLoading.subscribe({
        next: () => {
          if (this.nextResolutionToLoad !== undefined) {
            const urlAndInfo = this.boulderLoaderService.getUrl(this.bloc, this.nextResolutionToLoad);
            this.nextResolutionToLoad = urlAndInfo.higherResolution;
            this.boulderUrl = urlAndInfo.url;

            if (this.boulderUrl.length > 0) {
              this.boulderLoaderService.loadBoulder(this.boulderUrl).subscribe({
                next: (data: ArrayBuffer) => {
                  this.currentRawModel = data;
                  this.finishedLoading.next();
                },
              });
            }
          }
        }
      }));

      const urlAndInfo = this.boulderLoaderService.getUrl(this.bloc);
      this.nextResolutionToLoad = urlAndInfo.higherResolution;
      this.boulderUrl = urlAndInfo.url;
      this.finishedLoading.next();
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  // private loadBoulder(): void {
  //   this.defaultService.loadTestDaoneBoulder2('medium').subscribe({
  //   next: (data) => {
  //   this.currentRawModel = data;
  //   this.finishedLoading.next('medium');
  //   }
  //   });
  // }
}

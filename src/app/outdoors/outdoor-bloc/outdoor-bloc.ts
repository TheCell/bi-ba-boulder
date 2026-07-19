import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { EnhancedLine, OutdoorRenderer } from '../../renderer/outdoor-renderer/outdoor-renderer';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { BlocDto, LineDto, LinesService } from '@api-net/index';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subject, Subscription, switchMap } from 'rxjs';
import { ResolutionLevel } from '../../interfaces/resolution-level';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { CameraControls } from '../../spraywalls/spraywall/camera-controls/camera-controls';
import { ToastService } from '../../core/toast-container/toast.service';
import { BlocLineItem } from './bloc-line-item/bloc-line-item';
import { ColorService } from '../../core/util-services/color.service';

@Component({
  selector: 'app-outdoor-bloc',
  imports: [OutdoorRenderer, LoadingImageComponent, CameraControls, RouterLink, BlocLineItem],
  templateUrl: './outdoor-bloc.html',
  styleUrl: './outdoor-bloc.scss'
})
export class OutdoorBloc implements OnInit {
  private boulderLoaderService = inject(BoulderLoaderService);
  private linesService = inject(LinesService);
  private toastService = inject(ToastService);
  private colorService = inject(ColorService);

  public currentRawModel = signal<ArrayBuffer | undefined>(undefined);
  public bloc: BlocDto;
  public lines = signal<LineDto[]>([]);
  public enhancedLines = computed<EnhancedLine[]>(() => {
    const lines = this.lines();
    const enhancedLines = lines.map((line) => {
      const enhancedLine: EnhancedLine = {
        ...line,
        lineColor: this.colorService.nextColor()
      };
      return enhancedLine;
    });
    return enhancedLines;
  });
  public selectedLine = signal<{ line: LineDto; setFocus: boolean } | undefined>(undefined);

  private loadNextResolution = new Subject<void>();
  private startLoadingBoulder = new Subject<void>();
  private subscription = new Subscription();
  private boulderUrl = '';
  private resolutionToLoad?: ResolutionLevel;

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.bloc = activatedRoute.snapshot.data['bloc'];

    // todo cache and use cached if exists
    this.subscription.add(
      this.loadNextResolution.subscribe({
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
      })
    );

    this.subscription.add(
      this.startLoadingBoulder.pipe(switchMap(() => this.boulderLoaderService.loadBoulder(this.boulderUrl))).subscribe({
        next: (data: ArrayBuffer) => {
          this.currentRawModel.set(data);
          this.loadNextResolution.next();
        }
      })
    );

    const urlAndInfo = this.boulderLoaderService.getUrl(this.bloc);
    this.resolutionToLoad = urlAndInfo.higherResolution;
    this.boulderUrl = urlAndInfo.url;
    this.startLoadingBoulder.next();
  }

  public ngOnInit(): void {
    this.linesService.getLinesByBlocId(this.bloc.id).subscribe({
      next: (lines) => {
        this.lines.set(lines);
      }
    });
  }

  public onSelectedLine(line: { line: LineDto; setFocus: boolean } | undefined): void {
    if (this.selectedLine() === line) {
      this.selectedLine.set(undefined);
    } else {
      this.selectedLine.set(line);
    }
  }
}

import { Component, inject, signal } from '@angular/core';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { BlocDto } from '@api-net/index';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subject, Subscription, switchMap } from 'rxjs';
import { ResolutionLevel } from '../../interfaces/resolution-level';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { CameraControls } from '../../spraywalls/spraywall/camera-controls/camera-controls';
import { OutdoorEditorRenderer } from '../../renderer/outdoor-editor-renderer/outdoor-editor-renderer';

@Component({
  selector: 'app-outdoor-editor',
  imports: [LoadingImageComponent, CameraControls, OutdoorEditorRenderer, RouterLink],
  templateUrl: './outdoor-editor.html',
  styleUrl: './outdoor-editor.scss'
})
export class OutdoorEditor {
  private boulderLoaderService = inject(BoulderLoaderService);
  public currentRawModel = signal<ArrayBuffer | undefined>(undefined);
  public bloc: BlocDto;

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
}


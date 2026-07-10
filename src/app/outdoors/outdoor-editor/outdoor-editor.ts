import { Component, inject, signal, ViewChild } from '@angular/core';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { BlocDto, CreateLineCommand, LineData, LineDto, LinesService } from '@api-net/index';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject, Subscription, switchMap } from 'rxjs';
import { ResolutionLevel } from '../../interfaces/resolution-level';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { CameraControls } from '../../spraywalls/spraywall/camera-controls/camera-controls';
import { OutdoorEditorRenderer } from '../../renderer/outdoor-editor-renderer/outdoor-editor-renderer';
import { ToastService } from '../../core/toast-container/toast.service';

@Component({
  selector: 'app-outdoor-editor',
  imports: [LoadingImageComponent, CameraControls, OutdoorEditorRenderer, RouterLink],
  templateUrl: './outdoor-editor.html',
  styleUrl: './outdoor-editor.scss'
})
export class OutdoorEditor {
  @ViewChild('renderer') private renderer!: OutdoorEditorRenderer;

  private boulderLoaderService = inject(BoulderLoaderService);
  private linesService = inject(LinesService);
  private toastService = inject(ToastService);
  private router = inject(Router);

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

  public debugSaveRoute(): void {
    const linePoints = this.renderer.getLinePoints();
    if (!linePoints) {
      this.toastService.showDanger('Debug Save', 'No line data from renderer. Cannot save route.');
      throw new Error('No line data from renderer');
    }

    const lineData: LineData = {
      positions: linePoints
    };

    const createLine: CreateLineCommand = {
      blocId: this.bloc.id,
      identifier: 'debug-save-' + Date.now(),
      name: 'Debug Save',
      description: 'Debug save route',
      fontGrade: 5,
      data: lineData
    };
    console.log(createLine);

    this.linesService.createLineForBloc(this.bloc.id, createLine).subscribe({
      next: (_: LineDto) => {
        this.toastService.showSuccess('Debug Save', 'Debug save route clicked. Implement saving logic here.');
        this.router.navigate(['/', 'boulder', this.bloc.id]);
      },
      error: (error: Error) => {
        this.toastService.showDanger('Debug Save', 'Error saving route: ' + error.message);
      }
    });
  }
}


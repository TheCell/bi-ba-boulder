import { Component, computed, DestroyRef, inject, OnDestroy, signal, ViewChild } from '@angular/core';
import { SocialsOverlay } from '../../render-overlays/socials-overlay/socials-overlay';
import { EnhancedLine, OutdoorRenderer } from '../../renderer/outdoor-renderer/outdoor-renderer';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { BlocDto, LineDto, LinesService } from '@api-net/index';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map, merge, Subject, Subscription, switchMap, tap, toArray } from 'rxjs';
import { RESOLUTION_LEVEL, ResolutionLevel } from '../../interfaces/resolution-level';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { ToastService } from '../../core/toast-container/toast.service';
import { BlocLineItem } from './bloc-line-item/bloc-line-item';
import { ColorService } from '../../core/util-services/color.service';
import { Modal } from '../../core/modal/modal/modal';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ModalService } from '../../core/modal/modal.service';
import { CameraControls } from '../../render-overlays/camera-controls/camera-controls';
import { RawModelInput } from '../../renderer/outdoor-renderer/model-input.interface';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ConfirmDeleteOutdoorsDialog } from '../confirm-delete-outdoors-dialog/confirm-delete-outdoors-dialog';
import { ConfirmDeleteOutdoorsDialogData } from '../confirm-delete-outdoors-dialog/confirm-delete-outdoors-dialog-data';
import { Icon } from '../../core/icon/icon';

@Component({
  selector: 'app-outdoor-bloc',
  imports: [
    OutdoorRenderer,
    LoadingImageComponent,
    CameraControls,
    RouterLink,
    BlocLineItem,
    Modal,
    SocialsOverlay,
    Icon
  ],
  templateUrl: './outdoor-bloc.html',
  styleUrl: './outdoor-bloc.scss'
})
export class OutdoorBloc implements OnDestroy {
  @ViewChild('confirmDelete') private confirmDeleteModal!: Modal;

  private boulderLoaderService = inject(BoulderLoaderService);
  private linesService = inject(LinesService);
  private toastService = inject(ToastService);
  private colorService = inject(ColorService);
  private router = inject(Router);
  private modalService = inject(ModalService);
  private destroyRef = inject(DestroyRef);
  private activatedRoute = inject(ActivatedRoute);

  public currentRawModels = signal<RawModelInput[]>([]);
  public bloc: BlocDto;
  public previousBloc?: BlocDto;
  public nextBloc?: BlocDto;
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
  private selectedLineIdFromQueryParam?: string;

  private loadNextResolution = new Subject<ResolutionLevel>();
  private startLoadingBoulder = new Subject<{
    urls: string[];
    blocIds: string[];
    resolution: ResolutionLevel;
  }>();
  private blocChanged = new Subject<BlocDto>();
  private subscription = new Subscription();

  public constructor() {
    this.bloc = this.activatedRoute.snapshot.data['bloc'];

    this.subscription.add(
      this.activatedRoute.queryParamMap.subscribe({
        next: (queryParams) => {
          this.selectedLineIdFromQueryParam = queryParams.get('routeId') ?? undefined;
          if (this.lines().length > 0) {
            this.trySelectLineFromQueryParam();
          }
        }
      })
    );

    this.subscription.add(
      this.blocChanged.pipe(switchMap((bloc: BlocDto) => this.linesService.getLinesByBlocId(bloc.id))).subscribe({
        next: (lines: LineDto[]) => {
          this.lines.set(lines);
          this.trySelectLineFromQueryParam();
        }
      })
    );

    this.subscription.add(
      this.loadNextResolution.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (currentResolution) => {
          const nextResolution = this.boulderLoaderService.getNextResolution(this.bloc, currentResolution);
          if (nextResolution !== undefined) {
            const urlsAndInfo = this.boulderLoaderService.getUrls(this.bloc, nextResolution);
            if (urlsAndInfo.currentResolution !== undefined && urlsAndInfo.urls.length > 0) {
              this.startLoadingBoulder.next({
                urls: urlsAndInfo.urls,
                blocIds: urlsAndInfo.blocIds,
                resolution: urlsAndInfo.currentResolution
              });
            }
          }
        }
      })
    );

    // this.subscription.add(
    //   this.startLoadingBoulder
    //     .pipe(
    //       takeUntilDestroyed(this.destroyRef),
    //       switchMap(({ urls, blocIds, resolution }) => {
    //         const urlBlocPair = urls.map((url, index) => ({ url, blocId: blocIds[index] }));
    //         // todo load the first part without waiting for the additional parts
    //         return forkJoin(
    //           urlBlocPair.map(({ url, blocId }) => this.boulderLoaderService.loadBoulder(url, blocId, resolution))
    //         ).pipe(
    //           map((results) => {
    //             return { data: results, resolution, blocIds };
    //           })
    //         );
    //       })
    //     )
    //     .subscribe({
    //       next: ({
    //         data,
    //         resolution,
    //         blocIds
    //       }: {
    //         data: ArrayBuffer[];
    //         resolution: ResolutionLevel;
    //         blocIds: string[];
    //       }) => {
    //         const currentModels = [...(this.currentRawModels() ?? [])];
    //         for (let i = 0; i < data.length; i++) {
    //           currentModels.push({ arrayBuffer: data[i], resolution: resolution, blocId: blocIds[i] });
    //         }
    //         this.currentRawModels.set(currentModels);
    //         this.loadNextResolution.next(resolution);
    //       }
    //     })
    // );

    this.subscription.add(
      this.startLoadingBoulder
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          switchMap(({ urls, blocIds, resolution }) => {
            const urlBlocPair = urls.map((url, index) => ({ url, blocId: blocIds[index] }));

            return merge(
              ...urlBlocPair.map(({ url, blocId }) =>
                this.boulderLoaderService
                  .loadBoulder(url, blocId, resolution)
                  .pipe(map((result) => ({ result, blocId, resolution })))
              )
            ).pipe(
              tap(({ result, blocId, resolution }) => {
                // console.log(result);
                const currentModels = [...(this.currentRawModels() ?? [])];
                currentModels.push({ arrayBuffer: result, resolution: resolution, blocId: blocId });
                this.currentRawModels.set(currentModels);
              }),
              toArray(),
              map((results) => {
                return results[0].resolution;
              })
            );
            // .pipe(
            //   map((results) => {
            //     return results;
            //     // return { data: results, resolution, blocIds };
            //   })
            // );
          })
        )
        .subscribe({
          next: (resolution) => {
            // console.log('ye done', resolution);
            this.loadNextResolution.next(resolution);
          }
        })
      // .subscribe({
      //   next: ({
      //     data,
      //     resolution,
      //     blocIds
      //   }: {
      //     data: ArrayBuffer[];
      //     resolution: ResolutionLevel;
      //     blocIds: string[];
      //   }) => {
      //     console.log('ye done', blocIds);
      //     this.loadNextResolution.next(resolution);
      //   }
      // })
    );

    this.subscription.add(
      this.activatedRoute.data.subscribe({
        next: (data) => {
          const bloc = data['bloc'] as BlocDto;
          const blocs = (data['blocs'] as BlocDto[] | undefined) ?? [];
          this.loadBloc(bloc, blocs);
        }
      })
    );
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public onEditLine(): void {
    if (this.selectedLine() !== undefined) {
      this.router.navigate(['/', 'bloc-editor', this.bloc.id, this.selectedLine()!.line.id]);
    }
  }

  public onDeleteLine(): void {
    if (this.selectedLine()?.line) {
      const modal = this.modalService.open(this.confirmDeleteModal.id, ConfirmDeleteOutdoorsDialog);
      if (modal && modal.initialize) {
        const data: ConfirmDeleteOutdoorsDialogData = {
          line: this.selectedLine()!.line
        };
        modal.initialize(data);
      }
    }
  }

  public onDeleteProblemConfirmed(closeModalEvent: CloseModalEvent): void {
    if (closeModalEvent.closeType === 0) {
      if (this.selectedLine()?.line) {
        this.linesService.deleteLine(this.selectedLine()!.line.id).subscribe({
          next: () => {
            this.toastService.showSuccess('Success', 'Line successfully deleted');
            this.lines.set(this.lines().filter((l) => l.id !== this.selectedLine()!.line.id));
            this.selectedLine.set(undefined);
          }
        });
      }
    }
  }

  public onSelectedLine(line: { line: LineDto; setFocus: boolean } | undefined): void {
    if (line === undefined) {
      this.setSelectedLine(undefined);
      return;
    }

    if (this.selectedLine()?.line.id === line.line.id) {
      this.setSelectedLine(undefined);
    } else {
      this.setSelectedLine(line);
    }
  }

  public selectedRouteUrl(): string | undefined {
    const selectedLine = this.selectedLine();
    if (!selectedLine) {
      return undefined;
    }

    const urlTree = this.router.createUrlTree([], {
      relativeTo: this.activatedRoute,
      queryParams: { routeId: selectedLine.line.id },
      queryParamsHandling: 'merge'
    });

    return new URL(this.router.serializeUrl(urlTree), window.location.origin).toString();
  }

  public blocRouterLink(blocId: string): readonly string[] {
    const outdoorAreaId = this.activatedRoute.snapshot.paramMap.get('outdoorAreaId');
    const sectorId = this.activatedRoute.snapshot.paramMap.get('sectorId');

    if (outdoorAreaId && sectorId) {
      return ['/', 'outdoor-area', outdoorAreaId, 'sector', sectorId, 'bloc', blocId];
    }

    return ['/', 'bloc', blocId];
  }

  private setSelectedLine(selectedLine: { line: LineDto; setFocus: boolean } | undefined, updateUrl = true): void {
    this.selectedLine.set(selectedLine);
    if (updateUrl) {
      this.updateRouteSelectionInUrl(selectedLine?.line.id);
    }
  }

  private trySelectLineFromQueryParam(): void {
    const routeId = this.selectedLineIdFromQueryParam;
    const selectedLine = this.selectedLine();
    if (!routeId) {
      if (selectedLine) {
        this.setSelectedLine(undefined, false);
      }

      return;
    }

    if (selectedLine?.line.id === routeId) {
      return;
    }

    const lineFromList = this.lines().find((line) => line.id === routeId);
    if (lineFromList) {
      this.setSelectedLine({ line: lineFromList, setFocus: true }, false);
      return;
    }
  }

  private updateRouteSelectionInUrl(routeId?: string): void {
    this.router.navigate([], {
      queryParams: { routeId: routeId ?? null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  private loadBloc(bloc: BlocDto, blocs: BlocDto[]): void {
    this.bloc = bloc;
    this.currentRawModels.set([]);
    this.lines.set([]);
    this.selectedLine.set(undefined);

    setTimeout(() => {
      this.configureBlocNavigation(blocs);
      this.blocChanged.next(bloc);

      const urlsAndInfo = this.boulderLoaderService.getUrls(bloc, RESOLUTION_LEVEL.low);
      if (urlsAndInfo.urls.length > 0 && urlsAndInfo.currentResolution !== undefined) {
        this.startLoadingBoulder.next({
          urls: urlsAndInfo.urls,
          blocIds: urlsAndInfo.blocIds,
          resolution: urlsAndInfo.currentResolution
        });
      }
    });
  }

  private configureBlocNavigation(blocs: BlocDto[]): void {
    const currentIndex = blocs.findIndex((bloc: BlocDto): boolean => bloc.id === this.bloc.id);
    if (blocs.length < 2 || currentIndex < 0) {
      this.previousBloc = undefined;
      this.nextBloc = undefined;
      return;
    }

    this.previousBloc = blocs[(currentIndex - 1 + blocs.length) % blocs.length];
    this.nextBloc = blocs[(currentIndex + 1) % blocs.length];
  }
}

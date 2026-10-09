import {
  AfterViewInit,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  OnDestroy,
  signal,
  viewChild
} from '@angular/core';
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
import { SwipeIndicator } from '../../common/swipe-indicator/swipe-indicator';
import { SwipeEvent } from '../../common/swipe-indicator/swipe-event';

type EnhancedSwipeType = ({ [K in keyof SwipeEvent]: SwipeEvent[K] } & { identifier: number }) | undefined;

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
    Icon,
    SwipeIndicator
  ],
  templateUrl: './outdoor-bloc.html',
  styleUrl: './outdoor-bloc.scss'
})
export class OutdoorBloc implements AfterViewInit, OnDestroy {
  private confirmDeleteModal = viewChild.required<Modal>('confirmDelete');
  private legendSection = viewChild.required<ElementRef>('legendSection');

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
  private _swipeEvent = signal<EnhancedSwipeType>(undefined);
  public swipeEvent = this._swipeEvent.asReadonly();
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
                const currentModels = [...(this.currentRawModels() ?? [])];
                currentModels.push({ arrayBuffer: result, resolution: resolution, blocId: blocId });
                this.currentRawModels.set(currentModels);
              }),
              toArray(),
              map((results) => {
                return results[0].resolution;
              })
            );
          })
        )
        .subscribe({
          next: (resolution) => {
            this.loadNextResolution.next(resolution);
          }
        })
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

  public ngAfterViewInit(): void {
    this.legendSection().nativeElement.addEventListener('touchstart', this.touchSwipeStartEvent);
    this.legendSection().nativeElement.addEventListener('touchmove', this.touchSwipeMoveEvent);
    this.legendSection().nativeElement.addEventListener('touchend', this.touchSwipeEndEvent);
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.legendSection().nativeElement.removeEventListener('touchstart', this.touchSwipeStartEvent);
    this.legendSection().nativeElement.removeEventListener('touchmove', this.touchSwipeMoveEvent);
    this.legendSection().nativeElement.removeEventListener('touchend', this.touchSwipeEndEvent);
  }

  public onSwipe(event: number) {
    if (event > 0 && this.previousBloc !== undefined) {
      this.router.navigate(this.blocRouterLink(this.previousBloc.id));
    } else if (event < 0 && this.nextBloc !== undefined) {
      this.router.navigate(this.blocRouterLink(this.nextBloc.id));
    }
  }

  public onEditLine(): void {
    if (this.selectedLine() !== undefined) {
      this.router.navigate(['/', 'bloc-editor', this.bloc.id, this.selectedLine()!.line.id]);
    }
  }

  public onDeleteLine(): void {
    if (this.selectedLine()?.line) {
      const modal = this.modalService.open(this.confirmDeleteModal().id, ConfirmDeleteOutdoorsDialog);
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

  private touchSwipeStartEvent = (event: TouchEvent): void => {
    const touch = event.changedTouches.item(0);
    if (touch) {
      this._swipeEvent.set({ identifier: touch.identifier, startX: touch.clientX, endX: touch.clientX });
    }
  };

  private touchSwipeMoveEvent = (event: TouchEvent): void => {
    for (let i = 0; i < event.changedTouches.length; i++) {
      const touch = event.changedTouches.item(i);
      const swipeEvent = this._swipeEvent();
      if (touch && swipeEvent && swipeEvent.identifier === touch.identifier) {
        this._swipeEvent.set({ ...this._swipeEvent()!, endX: touch.clientX });
      }
    }
  };

  private touchSwipeEndEvent = (event: TouchEvent): void => {
    for (let i = 0; i < event.changedTouches.length; i++) {
      const touch = event.changedTouches.item(i);
      const swipeEvent = this._swipeEvent();
      if (touch && swipeEvent && swipeEvent.identifier === touch.identifier) {
        this._swipeEvent.set(undefined);
      }
    }
  };

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

import { Component, computed, inject, OnDestroy, OnInit, signal, ViewChild } from '@angular/core';
import { SocialsOverlay } from '../../render-overlays/socials-overlay/socials-overlay';
import { EnhancedLine, OutdoorRenderer } from '../../renderer/outdoor-renderer/outdoor-renderer';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { BlocDto, LineDto, LinesService } from '@api-net/index';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject, Subscription, switchMap } from 'rxjs';
import { ResolutionLevel } from '../../interfaces/resolution-level';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { ToastService } from '../../core/toast-container/toast.service';
import { BlocLineItem } from './bloc-line-item/bloc-line-item';
import { ColorService } from '../../core/util-services/color.service';
import { Modal } from '../../core/modal/modal/modal';
import { ConfirmDeleteDialog } from '../confirm-delete-dialog/confirm-delete-dialog';
import { ConfirmDeleteDialogData } from '../confirm-delete-dialog/confirm-delete-dialog-data';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ModalService } from '../../core/modal/modal.service';
import { CameraControls } from '../../render-overlays/camera-controls/camera-controls';

@Component({
  selector: 'app-outdoor-bloc',
  imports: [OutdoorRenderer, LoadingImageComponent, CameraControls, RouterLink, BlocLineItem, Modal, SocialsOverlay],
  templateUrl: './outdoor-bloc.html',
  styleUrl: './outdoor-bloc.scss'
})
export class OutdoorBloc implements OnInit, OnDestroy {
  @ViewChild('confirmDelete') private confirmDeleteModal!: Modal;

  private boulderLoaderService = inject(BoulderLoaderService);
  private linesService = inject(LinesService);
  private toastService = inject(ToastService);
  private colorService = inject(ColorService);
  private router = inject(Router);
  private modalService = inject(ModalService);

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
  private selectedLineIdFromQueryParam?: string;

  private loadNextResolution = new Subject<void>();
  private startLoadingBoulder = new Subject<void>();
  private subscription = new Subscription();
  private boulderUrl = '';
  private resolutionToLoad?: ResolutionLevel;

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.bloc = activatedRoute.snapshot.data['bloc'];

    this.subscription.add(
      activatedRoute.queryParamMap.subscribe({
        next: (queryParams) => {
          this.selectedLineIdFromQueryParam = queryParams.get('routeId') ?? undefined;
          this.trySelectLineFromQueryParam();
        }
      })
    );

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
        this.trySelectLineFromQueryParam();
      }
    });
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
      const modal = this.modalService.open(this.confirmDeleteModal.id, ConfirmDeleteDialog);
      if (modal && modal.initialize) {
        const data: ConfirmDeleteDialogData = {
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

    const urlTree = this.router.createUrlTree(['/', 'bloc', this.bloc.id], {
      queryParams: { routeId: selectedLine.line.id }
    });

    return new URL(this.router.serializeUrl(urlTree), window.location.origin).toString();
  }

  private setSelectedLine(selectedLine: { line: LineDto; setFocus: boolean } | undefined, updateUrl = true): void {
    this.selectedLine.set(selectedLine);
    if (updateUrl) {
      this.updateRouteSelectionInUrl(selectedLine?.line.id);
    }
  }

  private trySelectLineFromQueryParam(): void {
    const routeId = this.selectedLineIdFromQueryParam;
    if (!routeId) {
      if (this.selectedLine()) {
        this.setSelectedLine(undefined, false);
      }
      return;
    }

    if (this.selectedLine()?.line.id === routeId) {
      return;
    }

    const lineFromList = this.lines().find((line) => line.id === routeId);
    if (lineFromList) {
      this.setSelectedLine({ line: lineFromList, setFocus: true }, false);
      return;
    }

    this.linesService.getLine(routeId).subscribe({
      next: (line) => {
        this.setSelectedLine({ line, setFocus: true }, false);
      },
      error: () => {
        this.setSelectedLine(undefined, false);
      }
    });
  }

  private updateRouteSelectionInUrl(routeId?: string): void {
    this.router.navigate([], {
      queryParams: { routeId: routeId ?? null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }
}

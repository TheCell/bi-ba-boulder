import { Component, inject, signal, ViewChild } from '@angular/core';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { BlocDto, LineData, LineDto, LinesService } from '@api-net/index';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject, Subscription, switchMap } from 'rxjs';
import { RESOLUTION_LEVEL, ResolutionLevel } from '../../interfaces/resolution-level';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import {
  HelperTransformMode,
  InteractionMode,
  OutdoorEditorRenderer
} from '../../renderer/outdoor-editor-renderer/outdoor-editor-renderer';
import {
  OutdoorBlocMarkingsType,
  outdoorBlocMarkingColorOptions,
  OutdoorMarkingTypeAndColor
} from '../../renderer/common/outdoor-bloc-markings-types';
import { ToastService } from '../../core/toast-container/toast.service';
import { Modal } from '../../core/modal/modal/modal';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ModalService } from '../../core/modal/modal.service';
import { OutdoorSaveDialog } from '../outdoor-save-dialog/outdoor-save-dialog';
import { OutdoorSaveData } from '../outdoor-save-dialog/outdoor-save-data.interface';
import { CameraControls } from '../../render-overlays/camera-controls/camera-controls';
import { RawModelInput } from '../../renderer/outdoor-renderer/model-input.interface';

@Component({
  selector: 'app-outdoor-editor',
  imports: [LoadingImageComponent, CameraControls, OutdoorEditorRenderer, RouterLink, Modal],
  templateUrl: './outdoor-editor.html',
  styleUrl: './outdoor-editor.scss'
})
export class OutdoorEditor {
  @ViewChild('modal') private modal!: Modal;
  @ViewChild('renderer') private renderer!: OutdoorEditorRenderer;

  private boulderLoaderService = inject(BoulderLoaderService);
  private linesService = inject(LinesService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private modalService = inject(ModalService);

  public currentRawModels = signal<RawModelInput[]>([]);
  public bloc: BlocDto;
  public lineId? = '';
  public lineForEdit = signal<LineDto | undefined>(undefined);
  public revertLastPointCommand = signal(0);
  public selectedInteractionMode = signal<InteractionMode>('line');
  public selectedTransformMode = signal<HelperTransformMode>('rotate');
  public selectedBlocMarkingsType = signal<OutdoorBlocMarkingsType>(OutdoorBlocMarkingsType.start);
  public blocMarkingsTypeFormId = ''.appendUniqueId();
  public readonly blocMarkingsTypeOptions: OutdoorMarkingTypeAndColor[] = outdoorBlocMarkingColorOptions;

  private loadNextResolution = new Subject<ResolutionLevel>();
  private startLoadingBoulder = new Subject<{ url: string; resolution: ResolutionLevel }>();
  private subscription = new Subscription();
  // private boulderUrl = '';
  // private resolutionToLoad?: ResolutionLevel;

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.bloc = activatedRoute.snapshot.data['bloc'];
    this.lineForEdit.set(activatedRoute.snapshot.data['line']);

    const lineForEdit = this.lineForEdit();
    if (lineForEdit) {
      this.lineId = lineForEdit.id;
    }

    this.subscription.add(
      this.loadNextResolution.subscribe({
        next: (resolution) => {
          const nextResolution = this.boulderLoaderService.getNextResolution(this.bloc, resolution);
          if (nextResolution !== undefined) {
            const urlAndInfo = this.boulderLoaderService.getUrl(this.bloc, nextResolution);
            if (urlAndInfo.currentResolution !== undefined) {
              if (urlAndInfo.url.length > 0 && urlAndInfo.currentResolution !== undefined) {
                this.startLoadingBoulder.next({ url: urlAndInfo.url, resolution: urlAndInfo.currentResolution });
              }
            }
          }
        }
      })
    );

    this.subscription.add(
      this.startLoadingBoulder
        .pipe(
          switchMap(({ url, resolution }) =>
            this.boulderLoaderService.loadBoulder(url).pipe(
              switchMap((data) => {
                return [{ data, resolution }];
              })
            )
          )
        )
        .subscribe({
          next: ({ data, resolution }: { data: ArrayBuffer; resolution: ResolutionLevel }) => {
            const currentModels = [...(this.currentRawModels() ?? [])];
            currentModels.push({ arrayBuffer: data, resolution: resolution, blocId: this.bloc.id });
            this.currentRawModels.set(currentModels);
            this.loadNextResolution.next(resolution);
          }
        })
    );

    // const bestCached = this.boulderLoaderService.getBestCachedResolution(this.bloc);
    // console.log('bestCached: ', bestCached);

    const urlAndInfo = this.boulderLoaderService.getUrl(this.bloc, RESOLUTION_LEVEL.low);
    // this.boulderUrl = urlAndInfo.url;
    if (urlAndInfo.url.length > 0 && urlAndInfo.currentResolution !== undefined) {
      this.startLoadingBoulder.next({ url: urlAndInfo.url, resolution: urlAndInfo.currentResolution });
    }
  }

  public closeModal(closeModalEvent: CloseModalEvent) {
    if (closeModalEvent.closeType > 0) {
      // don't reset
    } else {
      const routeId = (closeModalEvent.data as { routeId?: string } | undefined)?.routeId;
      this.router.navigate(['/', 'bloc', this.bloc.id], {
        queryParams: { routeId: routeId ?? null },
        queryParamsHandling: 'merge'
      });
    }
  }

  public openSaveModal(): void {
    const component = this.modalService.open(this.modal.id, OutdoorSaveDialog);
    if (!component) {
      throw new Error('Modal component not found');
    }
    const linePoints = this.renderer.getLinePoints();
    if (!linePoints) {
      this.toastService.showDanger('Debug Save', 'No line data from renderer. Cannot save route.');
      throw new Error('No line data from renderer');
    }

    const sceneMarkings = this.renderer.getSceneMarkings();

    const lineData: LineData = {
      positions: linePoints,
      sceneMarkings: sceneMarkings
    };

    const dialogData: OutdoorSaveData = {
      lineData: lineData,
      blocId: this.bloc.id
    };

    const lineForEdit = this.lineForEdit();
    if (lineForEdit) {
      dialogData.existingId = lineForEdit.id;
      dialogData.name = lineForEdit.name ?? undefined;
      dialogData.description = lineForEdit.description;
      dialogData.fontGrade = lineForEdit.fontGrade;
      dialogData.version = lineForEdit.version;
      dialogData.identifier = lineForEdit.identifier;
    }
    component.initialize!(dialogData);
  }

  public sendRevertLastPointSignal(): void {
    this.revertLastPointCommand.update((value) => value + 1);
  }

  public onInteractionModeChanged(event: Event): void {
    const selectedValue: string | undefined = (event.target as HTMLSelectElement | null)?.value;
    if (!selectedValue) {
      return;
    }

    if (
      selectedValue === 'line' ||
      selectedValue === 'sphere-marking' ||
      selectedValue === 'box-marking' ||
      selectedValue === 'select-helper'
    ) {
      this.selectedInteractionMode.set(selectedValue);
    }
  }

  public onTransformModeChanged(event: Event): void {
    const selectedValue: string | undefined = (event.target as HTMLSelectElement | null)?.value;
    if (!selectedValue) {
      return;
    }

    if (selectedValue === 'translate' || selectedValue === 'rotate' || selectedValue === 'scale') {
      this.selectedTransformMode.set(selectedValue);
    }
  }

  public onBlocMarkingsTypeChanged(event: Event): void {
    const selectedValue: string | undefined = (event.target as HTMLSelectElement | null)?.value;
    if (!selectedValue) {
      return;
    }

    const parsedType = Number(selectedValue);
    if (
      parsedType === OutdoorBlocMarkingsType.start ||
      parsedType === OutdoorBlocMarkingsType.top ||
      parsedType === OutdoorBlocMarkingsType.offLineZone
    ) {
      this.selectedBlocMarkingsType.set(parsedType);
    }
  }

  public isTransformModeDisabled(): boolean {
    return this.selectedInteractionMode() === 'line';
  }

  public isBlocMarkingsTypeDisabled(): boolean {
    return this.selectedInteractionMode() === 'line' || this.selectedInteractionMode() === 'select-helper';
  }

  public enumName(type: OutdoorBlocMarkingsType): string {
    switch (type) {
      case OutdoorBlocMarkingsType.start:
        return 'Start';
      case OutdoorBlocMarkingsType.top:
        return 'Top / Exit';
      case OutdoorBlocMarkingsType.offLineZone:
        return 'Off Line Zone';
      default:
        return 'Unknown';
    }
  }
}

import { Component, inject, signal, ViewChild } from '@angular/core';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { BlocDto, LineData, LineDto, LinesService } from '@api-net/index';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subject, Subscription, switchMap } from 'rxjs';
import { ResolutionLevel } from '../../interfaces/resolution-level';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { CameraControls } from '../../spraywalls/spraywall/camera-controls/camera-controls';
import { OutdoorEditorRenderer } from '../../renderer/outdoor-editor-renderer/outdoor-editor-renderer';
import { ToastService } from '../../core/toast-container/toast.service';
import { Modal } from '../../core/modal/modal/modal';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ModalService } from '../../core/modal/modal.service';
import { OutdoorSaveDialog } from '../outdoor-save-dialog/outdoor-save-dialog';
import { OutdoorSaveData } from '../outdoor-save-dialog/outdoor-save-data.interface';

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

  public currentRawModel = signal<ArrayBuffer | undefined>(undefined);
  public bloc: BlocDto;
  public lineId? = '';
  public lineForEdit = signal<LineDto | undefined>(undefined);
  public revertLastPointCommand = signal(0);

  private loadNextResolution = new Subject<void>();
  private startLoadingBoulder = new Subject<void>();
  private subscription = new Subscription();
  private boulderUrl = '';
  private resolutionToLoad?: ResolutionLevel;

  public constructor() {
    const activatedRoute = inject(ActivatedRoute);
    this.bloc = activatedRoute.snapshot.data['bloc'];
    this.lineForEdit.set(activatedRoute.snapshot.data['line']);

    const lineForEdit = this.lineForEdit();
    if (lineForEdit) {
      this.lineId = lineForEdit.id;
    }

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

  public closeModal(closeModalEvent: CloseModalEvent) {
    if (closeModalEvent.closeType > 0) {
      // don't reset
    } else {
      this.router.navigate(['/', 'bloc', this.bloc.id]);
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

    const lineData: LineData = {
      positions: linePoints
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
}


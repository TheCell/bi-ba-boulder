import {
  afterRenderEffect,
  ChangeDetectorRef,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  OnDestroy,
  OnInit,
  signal,
  viewChild
} from '@angular/core';
import { SocialsOverlay } from '../../render-overlays/socials-overlay/socials-overlay';
import { BoulderRenderComponent } from '../../renderer/boulder-render/boulder-render.component';
import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import {
  BoulderLogDto,
  BoulderLogsService,
  SearchProblemsRequest,
  SpraywallProblemDto,
  SpraywallProblemListDto,
  SpraywallProblemsService,
  SpraywallsService
} from '@api-net/index';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { SpraywallLegendItem } from './spraywall-legend-item/spraywall-legend-item';
import { catchError, debounceTime, EMPTY, filter, Subject, Subscription, switchMap } from 'rxjs';
import { SpraywallGradeFilterDialog } from './spraywall-grade-filter-dialog/spraywall-grade-filter-dialog';
import { SpraywallLegendItemPlaceholder } from './spraywall-legend-item-placeholder/spraywall-legend-item-placeholder';
import { SpraywallGradeFilterDialogCloseData } from './spraywall-grade-filter-dialog/spraywall-grade-filter-dialog-close-data';
import { SpraywallGradeFilterDialogData } from './spraywall-grade-filter-dialog/spraywall-grade-filter-dialog-data';
import { SpraywallInfoDialog } from './spraywall-info-dialog/spraywall-info-dialog';
import { ConfirmDeleteDialog } from './confirm-delete-spraywall-problem-dialog/confirm-delete-spraywall-problem-dialog';
import { ConfirmDeleteSpraywallProblemDialogData } from './confirm-delete-spraywall-problem-dialog/confirm-delete-spraywall-problem-dialog-data';
import { ProblemLogOverlay } from './problem-log-overlay/problem-log-overlay';
import { Icon } from '../../core/icon/icon';
import { Modal } from '../../core/modal/modal/modal';
import { AuthSessionStateService } from '../../auth/auth-session-state.service';
import { ModalService } from '../../core/modal/modal.service';
import { ToastService } from '../../core/toast-container/toast.service';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { ProblemRulesOverlay } from '../../render-overlays/problem-rules-overlay/problem-rules-overlay';
import { CameraControls } from '../../render-overlays/camera-controls/camera-controls';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-spraywall',
  imports: [
    LoadingImageComponent,
    BoulderRenderComponent,
    SpraywallLegendItem,
    SpraywallLegendItemPlaceholder,
    RouterLink,
    Icon,
    Modal,
    ProblemLogOverlay,
    CameraControls,
    ProblemRulesOverlay,
    SocialsOverlay
  ],
  templateUrl: './spraywall.component.html',
  styleUrl: './spraywall.component.scss'
})
export class SpraywallComponent implements OnInit, OnDestroy {
  private infoModal = viewChild.required<Modal>('infoModal');
  private fontGradeFilterModal = viewChild.required<Modal>('fontGradeFilterModal');
  private confirmDeleteModal = viewChild.required<Modal>('confirmDelete');
  private scrollList = viewChild.required<ElementRef<HTMLElement>>('scrollList');
  private loadingPlaceholder = viewChild<SpraywallLegendItemPlaceholder, ElementRef<HTMLElement>>(
    'loadingPlaceholder',
    {
      read: ElementRef
    }
  );

  public authSessionStateService = inject(AuthSessionStateService);
  private spraywallsService = inject(SpraywallsService);
  private spraywallProblemsService = inject(SpraywallProblemsService);
  private boulderLoaderService = inject(BoulderLoaderService);
  private modalService = inject(ModalService);
  private changeDetectorRef = inject(ChangeDetectorRef);
  private router = inject(Router);
  private toastService = inject(ToastService);
  private boulderLogsService = inject(BoulderLogsService);
  private activatedRoute = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  private data = toSignal(this.activatedRoute.data, { requireSync: true });
  public test = computed(() => console.log(this.data()), this.data());
  public spraywall = computed(() => this.data()['spraywall']);

  public currentRawModel?: ArrayBuffer;
  public spraywallId = this.activatedRoute.snapshot.params['id'];

  public listOfProblems = signal<SpraywallProblemDto[]>([]);
  public selectedProblem?: SpraywallProblemDto = undefined;
  public boulderLog = signal<BoulderLogDto | undefined>(undefined);
  public currentFilter: SearchProblemsRequest = {};
  private selectedProblemIdFromQueryParam?: string;

  public totalCount = signal(0);
  private currentMaxPage = signal(1);
  // private pageSize = 30;
  private newEntriesLoading = signal(true);

  private reloadSearchSubject = new Subject<void>();
  private loadBoulderLog = new Subject<string>();
  private subscription = new Subscription();

  public constructor() {
    afterRenderEffect((onCleanup: (cleanup: VoidFunction) => void): void => {
      const placeholder: ElementRef<HTMLElement> | undefined = this.loadingPlaceholder();

      this.listOfProblems();
      if (!placeholder) {
        return;
      }

      const observer: IntersectionObserver = new IntersectionObserver(
        (entries: IntersectionObserverEntry[]): void => {
          if (entries.some((entry: IntersectionObserverEntry): boolean => entry.isIntersecting)) {
            this.loadNextPage();
          }
        },
        { root: this.scrollList().nativeElement, rootMargin: '0px 0px 25px 0px' }
      );
      observer.observe(placeholder.nativeElement);
      onCleanup((): void => observer.disconnect());
    });

    this.subscription.add(
      this.activatedRoute.queryParamMap.subscribe({
        next: (queryParams) => {
          this.selectedProblemIdFromQueryParam = queryParams.get('routeId') ?? undefined;
          this.trySelectProblemFromQueryParam();
        }
      })
    );

    this.subscription.add(
      this.reloadSearchSubject
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          debounceTime(300),
          switchMap(() =>
            this.spraywallsService.searchSpraywallProblems(this.spraywallId, this.currentFilter).pipe(
              catchError((): typeof EMPTY => {
                this.newEntriesLoading.set(false);
                this.toastService.showDanger('Error', 'Could not load routes. Please try again.');
                return EMPTY;
              })
            )
          )
        )
        .subscribe({
          next: (problemSearchResult: SpraywallProblemListDto) => {
            this.newEntriesLoading.set(false);
            this.currentMaxPage.set(problemSearchResult.currentPage);
            if (problemSearchResult.currentPage > 1) {
              this.listOfProblems.set([...this.listOfProblems(), ...problemSearchResult.problems]);
            } else {
              this.listOfProblems.set(problemSearchResult.problems);
            }
            this.totalCount.set(problemSearchResult.totalCount);
            this.trySelectProblemFromQueryParam();
          }
        })
    );

    this.subscription.add(
      this.loadBoulderLog
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          filter(() => this.authSessionStateService.isLoggedIn()),
          switchMap((problemId) => this.boulderLogsService.getBoulderLogBySpraywall(problemId))
        )
        .subscribe({
          next: (boulderLog?: BoulderLogDto | null) => {
            if (boulderLog) {
              this.boulderLog.set(boulderLog);
            } else {
              this.boulderLog.set(undefined);
            }
          },
          error: () => {
            this.boulderLog.set(undefined);
          }
        })
    );
  }

  public ngOnInit() {
    this.boulderLoaderService.loadSpraywall(this.spraywall().spraywallModel, this.spraywallId).subscribe({
      next: (data: ArrayBuffer) => {
        this.currentRawModel = data;
        this.changeDetectorRef.markForCheck();
      }
    });

    this.reloadSearchSubject.next();
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public onSelectedProblem(problem: SpraywallProblemDto): void {
    if (this.selectedProblem?.id === problem.id) {
      this.onResetSelection();
    } else {
      this.setSelectedProblem(problem);
    }
  }

  public onResetSelection(updateUrl = true): void {
    this.selectedProblem = undefined;
    this.boulderLog.set(undefined);
    if (updateUrl) {
      this.updateRouteSelectionInUrl(undefined);
    }
  }

  public onDateClicked(): void {
    const currentOrder = this.currentFilter.dateOrder ?? undefined;
    if (currentOrder === 'desc') {
      this.currentFilter.dateOrder = undefined;
    } else if (currentOrder === 'asc') {
      this.currentFilter.dateOrder = 'desc';
    } else {
      this.currentFilter.dateOrder = 'asc';
    }

    this.resetFilterPageAndResultsPosition();
    this.reloadSearchSubject.next();
  }

  public hintAtNotLoggedIn(): void {
    this.toastService.showWarning('Not logged in', 'You must be logged in to add new Routes');
  }

  public onGradeClicked(): void {
    const modal = this.modalService.open(this.fontGradeFilterModal().id, SpraywallGradeFilterDialog);
    if (modal && modal.initialize) {
      const data: SpraywallGradeFilterDialogData = {
        maxGrade: this.currentFilter.gradeMax,
        minGrade: this.currentFilter.gradeMin
      };
      modal.initialize(data);
    }
  }

  public onFontGradeModalClosed(closeModalEvent: CloseModalEvent): void {
    if (closeModalEvent.closeType === 0) {
      const data = closeModalEvent.data as SpraywallGradeFilterDialogCloseData;
      this.currentFilter.gradeMax = data.maxGrade;
      this.currentFilter.gradeMin = data.minGrade;

      this.resetFilterPageAndResultsPosition();
      this.reloadSearchSubject.next();
    }
  }

  public onInfoClicked(): void {
    this.modalService.open(this.infoModal().id, SpraywallInfoDialog);
  }

  public onEditProblem(): void {
    if (this.selectedProblem) {
      this.router.navigate(['/', 'spraywall-editor', this.spraywallId, this.selectedProblem.id]);
    }
  }

  public onDeleteProblem(): void {
    if (this.selectedProblem) {
      const modal = this.modalService.open(this.confirmDeleteModal().id, ConfirmDeleteDialog);
      if (modal && modal.initialize) {
        const data: ConfirmDeleteSpraywallProblemDialogData = {
          spraywallProblem: this.selectedProblem
        };
        modal.initialize(data);
      }
    }
  }

  public onDeleteProblemConfirmed(closeModalEvent: CloseModalEvent): void {
    if (closeModalEvent.closeType === 0) {
      if (this.selectedProblem) {
        this.spraywallProblemsService.deleteProblem(this.selectedProblem.id).subscribe({
          next: () => {
            this.toastService.showSuccess('Success', 'Problem successfully deleted');
            this.onResetSelection();
            this.resetFilterPageAndResultsPosition();
            this.reloadSearchSubject.next();
          }
        });
      }
    }
  }

  private loadNextPage(): void {
    if (this.newEntriesLoading() || this.listOfProblems().length >= this.totalCount()) {
      return;
    }

    this.newEntriesLoading.set(true);
    this.currentFilter.page = this.currentMaxPage() + 1;
    this.reloadSearchSubject.next();
  }

  private resetFilterPageAndResultsPosition(): void {
    this.newEntriesLoading.set(true);
    this.currentMaxPage.set(1);
    this.currentFilter.page = 0;
    this.totalCount.set(0);
    this.listOfProblems.set([]);
  }

  public selectedRouteUrl(): string | undefined {
    if (!this.selectedProblem) {
      return undefined;
    }

    const urlTree = this.router.createUrlTree(['/', 'spraywall', this.spraywallId], {
      queryParams: { routeId: this.selectedProblem.id }
    });

    return new URL(this.router.serializeUrl(urlTree), window.location.origin).toString();
  }

  private setSelectedProblem(problem: SpraywallProblemDto, updateUrl = true): void {
    this.selectedProblem = problem;
    this.boulderLog.set(undefined);
    this.loadBoulderLog.next(problem.id);
    if (updateUrl) {
      this.updateRouteSelectionInUrl(problem.id);
    }
  }

  private trySelectProblemFromQueryParam(): void {
    const routeId = this.selectedProblemIdFromQueryParam;
    if (!routeId) {
      if (this.selectedProblem) {
        this.onResetSelection(false);
      }
      return;
    }

    if (this.selectedProblem?.id === routeId) {
      return;
    }

    const problemFromList = this.listOfProblems().find((problem) => problem.id === routeId);
    if (problemFromList) {
      this.setSelectedProblem(problemFromList, false);
      return;
    }

    this.spraywallProblemsService.getProblem(routeId).subscribe({
      next: (problem) => {
        this.setSelectedProblem(problem, false);
      },
      error: () => {
        this.onResetSelection(false);
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

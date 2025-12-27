import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { BoulderRenderComponent } from '../../renderer/boulder-render/boulder-render.component';

import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { PostSearchProblemsRequest, SpraywallProblemDto, SpraywallProblemSearchDto, SpraywallsService } from '@api/index';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { SpraywallLegendItem } from './spraywall-legend-item/spraywall-legend-item';
import { Icon } from 'src/app/core/icon/icon';
import { debounceTime, Subject, Subscription, switchMap } from 'rxjs';
import { ModalService } from 'src/app/core/modal/modal.service';
import { Modal } from 'src/app/core/modal/modal/modal';
import { SpraywallGradeFilterDialog } from './spraywall-grade-filter-dialog/spraywall-grade-filter-dialog';
import { SpraywallLegendItemPlaceholder } from './spraywall-legend-item-placeholder/spraywall-legend-item-placeholder';
import { CloseModalEvent } from 'src/app/core/modal/modal/close-modal-event';
import { SpraywallGradeFilterDialogCloseData } from './spraywall-grade-filter-dialog/spraywall-grade-filter-dialog-close-data';
import { SpraywallGradeFilterDialogData } from './spraywall-grade-filter-dialog/spraywall-grade-filter-dialog-data';
import { SpraywallInfoDialog } from './spraywall-info-dialog/spraywall-info-dialog';
import { LoginTrackerService } from 'src/app/auth/login-tracker.service';
import { ToastService } from 'src/app/core/toast-container/toast.service';

@Component({
  selector: 'app-spraywall',
  imports: [LoadingImageComponent, BoulderRenderComponent, SpraywallLegendItem, SpraywallLegendItemPlaceholder,
    RouterLink, Icon, Modal],
  templateUrl: './spraywall.component.html',
  styleUrl: './spraywall.component.scss',
  changeDetection: ChangeDetectionStrategy.Default
})
export class SpraywallComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('infoModal') private infoModal!: Modal;
  @ViewChild('fontGradeFilterModal') private fontGradeFilterModal!: Modal;
  @ViewChild('scrollList') private scrollList!: ElementRef<HTMLElement>;
  
  public loginTrackerService = inject(LoginTrackerService);
  private spraywallsService = inject(SpraywallsService);
  private boulderLoaderService = inject(BoulderLoaderService)
  private modalService = inject(ModalService);
  private changeDetectorRef = inject(ChangeDetectorRef);
  private router = inject(Router);
  private toastService = inject(ToastService);

  public currentRawModel?: ArrayBuffer;
  public spraywallId = '';

  public listOfProblems: SpraywallProblemDto[] = [];
  public selectedProblem?: SpraywallProblemDto = undefined;
  public currentFilter: PostSearchProblemsRequest = {};

  public totalCount = 0;
  private currentMaxPage = 1;
  // private pageSize = 30;
  private newEntriesLoading = false;
  
  private reloadSearchSubject = new Subject<void>();
  private subscription = new Subscription();

  public constructor() {
    const route = inject(ActivatedRoute);
    this.spraywallId = route.snapshot.params['id'];

    this.subscription.add(this.reloadSearchSubject.pipe(debounceTime(300), switchMap(() => this.spraywallsService.postSearchProblems(this.spraywallId, this.currentFilter))).subscribe({
      next: (problemSearchResult: SpraywallProblemSearchDto) => {
        this.newEntriesLoading = false;
        if (problemSearchResult.currentPage > 0) {
          this.listOfProblems.push(... problemSearchResult.problems);
        } else {
          this.listOfProblems = problemSearchResult.problems;
        }
        this.totalCount = problemSearchResult.totalCount;

        this.changeDetectorRef.markForCheck();
      }
    }));
  }

  public ngAfterViewInit(): void {
    this.scrollList.nativeElement.addEventListener('scroll', this.scrollEventListener);
  }

  public ngOnInit() {
    this.boulderLoaderService.loadTestSpraywall3().subscribe({
      next: (data: ArrayBuffer) => {
        this.currentRawModel = data;
        this.changeDetectorRef.markForCheck();
      }
    });

    this.reloadSearchSubject.next();
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.scrollList.nativeElement.removeEventListener('scroll', this.scrollEventListener);
  }
  
  public onSelectedProblem(problem: SpraywallProblemDto): void {
    if (this.selectedProblem?.id === problem.id) {
      this.onResetSelection();
    } else {
      this.selectedProblem = problem;
    }
  }

  public onResetSelection(): void {
    this.selectedProblem = undefined;
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
    const modal = this.modalService.open(this.fontGradeFilterModal.id, SpraywallGradeFilterDialog);
    if (modal && modal.initialize) {
      const data: SpraywallGradeFilterDialogData = {
        maxGrade: this.currentFilter.gradeMax,
        minGrade: this.currentFilter.gradeMin
      }
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
    this.modalService.open(this.infoModal.id, SpraywallInfoDialog);
  }

  public onEditProblem(): void {
    console.log('todo');
    if (this.selectedProblem) {
      this.router.navigate(['/', 'spraywall-editor', this.spraywallId, this.selectedProblem.id]);
    }
  }

  private scrollEventListener = () => {
    this.scrollEvent();
  }

  private scrollEvent(): void {
    if (this.currentScrollPosition() < 25 && this.listOfProblems.length < this.totalCount) {
      this.loadNextPage();
    }
  }

  private currentScrollPosition(): number {
    const element = this.scrollList.nativeElement;
    return Math.abs(element.scrollHeight - element.clientHeight - element.scrollTop);
  }

  private loadNextPage(): void {
    if (this.newEntriesLoading) {
      return;
    }

    this.newEntriesLoading = true;
    this.currentMaxPage = this.currentMaxPage + 1;
    this.currentFilter.page = this.currentMaxPage;
    this.reloadSearchSubject.next();
  }

  private resetFilterPageAndResultsPosition(): void {
    this.currentMaxPage = 1;
    this.currentFilter.page = 0;
    this.totalCount = 0;
    this.listOfProblems = [];
    // this.onResetSelection();
  }
}
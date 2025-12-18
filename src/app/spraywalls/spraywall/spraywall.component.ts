import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { BoulderRenderComponent } from '../../renderer/boulder-render/boulder-render.component';

import { LoadingImageComponent } from '../../common/loading-image/loading-image.component';
import { PostSearchProblemsRequest, SpraywallProblemDto, SpraywallProblemSearchDto, SpraywallsService } from '@api/index';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { SpraywallLegendItem } from './spraywall-legend-item/spraywall-legend-item';
import { Icon } from 'src/app/core/icon/icon';
import { debounceTime, Subject, Subscription, switchMap } from 'rxjs';
import { ModalService } from 'src/app/core/modal/modal.service';
import { Modal } from 'src/app/core/modal/modal/modal';
import { SpraywallGradeFilter } from './spraywall-grade-filter/spraywall-grade-filter';

@Component({
  selector: 'app-spraywall',
  imports: [LoadingImageComponent, BoulderRenderComponent, SpraywallLegendItem, RouterLink, Icon, Modal],
  templateUrl: './spraywall.component.html',
  styleUrl: './spraywall.component.scss',
  changeDetection: ChangeDetectionStrategy.Default
})
export class SpraywallComponent implements OnInit, OnDestroy {
  @ViewChild('fontGradeFilterModal') private fontGradeFilterModal!: Modal;

  private spraywallsService = inject(SpraywallsService);
  private boulderLoaderService = inject(BoulderLoaderService)
  private modalService = inject(ModalService);
  private changeDetectorRef = inject(ChangeDetectorRef);

  public currentRawModel?: ArrayBuffer;
  public spraywallId = '';

  public listOfProblems: SpraywallProblemDto[] = [];
  public selectedProblem?: SpraywallProblemDto = undefined;
  public currentFilter: PostSearchProblemsRequest = {};

  private reloadSearchSubject = new Subject<void>();
  private subscription = new Subscription();

  public constructor() {
    const route = inject(ActivatedRoute);
    this.spraywallId = route.snapshot.params['id'];

    this.subscription.add(this.reloadSearchSubject.pipe(debounceTime(300), switchMap(() => this.spraywallsService.postSearchProblems(this.spraywallId, this.currentFilter))).subscribe({
      next: (problemSearchResult: SpraywallProblemSearchDto) => {
        this.listOfProblems = problemSearchResult.problems
        this.changeDetectorRef.markForCheck();
      }
    }));
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
  }
  
  public onSelectedProblem(problem: SpraywallProblemDto): void {
    this.selectedProblem = problem;
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

    this.reloadSearchSubject.next();
  }

  public onGradeClicked(): void {
    this.modalService.open(this.fontGradeFilterModal.id, SpraywallGradeFilter);
  }
}
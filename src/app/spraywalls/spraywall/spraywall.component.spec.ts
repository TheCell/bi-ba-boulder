import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import {
  BoulderLogsService,
  SearchProblemsRequest,
  SpraywallProblemDto,
  SpraywallProblemListDto,
  SpraywallProblemsService,
  SpraywallsService
} from '@api-net/index';
import { Observable, of, Subject } from 'rxjs';
import { AuthSessionStateService } from '../../auth/auth-session-state.service';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { ModalService } from '../../core/modal/modal.service';
import { ToastService } from '../../core/toast-container/toast.service';
import { SpraywallComponent } from './spraywall.component';

interface ObservedPlaceholder {
  callback: IntersectionObserverCallback;
  observer: jasmine.SpyObj<IntersectionObserver>;
  target?: Element;
}

type SearchProblems = (id: string, request?: SearchProblemsRequest) => Observable<SpraywallProblemListDto>;

describe('SpraywallComponent pagination', (): void => {
  let fixture: ComponentFixture<SpraywallComponent>;
  let component: SpraywallComponent;
  let searchProblems: jasmine.Spy<SearchProblems>;
  let toastService: jasmine.SpyObj<ToastService>;
  let responses: Subject<SpraywallProblemListDto>[];
  let requestedPages: (number | null | undefined)[];
  let observations: ObservedPlaceholder[];
  let observerConstructor: jasmine.Spy<
    (callback: IntersectionObserverCallback, options?: IntersectionObserverInit) => IntersectionObserver
  >;

  const problem = (id: string): SpraywallProblemDto => ({
    id,
    name: id,
    image: '',
    createdById: 'creator',
    createdByName: 'Creator',
    createdDate: '2026-11-13',
    version: 1,
    metadata: { canEdit: false, canDelete: false }
  });

  const waitForRequest = async (count: number): Promise<void> => {
    const deadline: number = Date.now() + 1000;
    while (responses.length < count && Date.now() < deadline) {
      await new Promise<void>((resolve): void => {
        setTimeout(resolve, 10);
      });
    }
    expect(responses.length).toBe(count);
  };

  const renderPage = async (index: number, totalCount: number): Promise<void> => {
    responses[index].next({ currentPage: index + 1, totalCount, problems: [problem(`${index + 1}`)] });
    fixture.detectChanges();
    await fixture.whenStable();
  };

  const notifyVisibility = (isIntersecting: boolean): void => {
    const observation: ObservedPlaceholder = observations[observations.length - 1];
    if (!observation.target) {
      throw new Error('No loading placeholder is being observed');
    }
    const rect: DOMRect = new DOMRect();
    observation.callback(
      [
        {
          target: observation.target,
          isIntersecting,
          intersectionRatio: isIntersecting ? 1 : 0,
          boundingClientRect: rect,
          intersectionRect: rect,
          rootBounds: rect,
          time: 0
        }
      ],
      observation.observer
    );
  };

  beforeEach(async (): Promise<void> => {
    responses = [];
    requestedPages = [];
    observations = [];
    observerConstructor = spyOn(window, 'IntersectionObserver').and.callFake(function (
      callback: IntersectionObserverCallback,
      options?: IntersectionObserverInit
    ): IntersectionObserver {
      const observer: jasmine.SpyObj<IntersectionObserver> = jasmine.createSpyObj<IntersectionObserver>(
        'IntersectionObserver',
        ['observe', 'disconnect']
      );
      const observation: ObservedPlaceholder = { callback, observer };
      observer.observe.and.callFake((target: Element): void => {
        observation.target = target;
        expect(options?.root).toBe(fixture.nativeElement.querySelector('.list-content'));
      });
      observations.push(observation);
      return observer;
    });
    searchProblems = jasmine
      .createSpy<SearchProblems>('searchSpraywallProblems')
      .and.callFake((_id: string, request?: SearchProblemsRequest): Observable<SpraywallProblemListDto> => {
        const response: Subject<SpraywallProblemListDto> = new Subject<SpraywallProblemListDto>();
        responses.push(response);
        requestedPages.push(request?.page);
        return response;
      });
    toastService = jasmine.createSpyObj<ToastService>('ToastService', ['showDanger']);

    await TestBed.configureTestingModule({
      imports: [SpraywallComponent],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            data: of({ spraywall: { spraywallModel: 'model' } }),
            queryParamMap: of(convertToParamMap({})),
            snapshot: { params: { id: 'wall' } }
          }
        },
        { provide: SpraywallsService, useValue: { searchSpraywallProblems: searchProblems } },
        { provide: SpraywallProblemsService, useValue: {} },
        { provide: BoulderLogsService, useValue: {} },
        {
          provide: BoulderLoaderService,
          useValue: { loadSpraywall: (): Observable<ArrayBuffer> => of(new ArrayBuffer(0)) }
        },
        { provide: AuthSessionStateService, useValue: { isLoggedIn: (): boolean => false } },
        { provide: ModalService, useValue: {} },
        { provide: ToastService, useValue: toastService }
      ]
    })
      .overrideComponent(SpraywallComponent, {
        set: {
          imports: [],
          template: `
          <div #scrollList class="list-content" style="height: 100px; overflow-y: auto">
            @for (problem of listOfProblems(); track problem.id) {
              <div style="height: 20px">{{ problem.name }}</div>
            }
            @if (listOfProblems().length < totalCount()) {
              <div #loadingPlaceholder style="height: 20px">Loading</div>
            }
          </div>
        `
        }
      })
      .compileComponents();

    fixture = TestBed.createComponent(SpraywallComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await waitForRequest(1);
  });

  it('automatically fills the viewport using the browser observer without any scroll events', async (): Promise<void> => {
    observerConstructor.and.callThrough();
    await renderPage(0, 3);
    await waitForRequest(2);
    await renderPage(1, 3);
    await waitForRequest(3);
    await renderPage(2, 3);
    expect(requestedPages).toEqual([undefined, 2, 3]);
    expect(component.listOfProblems().length).toBe(3);
  });

  it('loads consecutive pages without scrolling while the placeholder remains visible', async (): Promise<void> => {
    await renderPage(0, 3);
    const target: Element | undefined = observations[0].target;
    notifyVisibility(true);
    notifyVisibility(true);
    await waitForRequest(2);
    expect(requestedPages[1]).toBe(2);

    await renderPage(1, 3);
    expect(observations[0].observer.disconnect).toHaveBeenCalled();
    expect(observations[1].target).toBe(target);
    notifyVisibility(true);
    await waitForRequest(3);
    expect(requestedPages[2]).toBe(3);

    await renderPage(2, 3);
    expect(component.listOfProblems().map((entry: SpraywallProblemDto): string => entry.id)).toEqual(['1', '2', '3']);
    expect(fixture.nativeElement.textContent).not.toContain('Loading');
    expect(observations[1].observer.disconnect).toHaveBeenCalled();
    notifyVisibility(true);
    await new Promise<void>((resolve): void => {
      setTimeout(resolve, 350);
    });
    expect(responses.length).toBe(3);
  });

  it('waits until the placeholder enters the list viewport and disconnects on destruction', async (): Promise<void> => {
    await renderPage(0, 2);
    notifyVisibility(false);
    await new Promise<void>((resolve): void => {
      setTimeout(resolve, 350);
    });
    expect(responses.length).toBe(1);
    fixture.destroy();
    expect(observations[0].observer.disconnect).toHaveBeenCalled();
  });

  it('restarts pagination after changing filters without loading from a stale observer', async (): Promise<void> => {
    await renderPage(0, 2);
    component.onDateClicked();
    notifyVisibility(true);
    await waitForRequest(2);
    expect(requestedPages[1]).toBe(0);

    responses[1].next({ currentPage: 1, totalCount: 2, problems: [problem('filtered')] });
    fixture.detectChanges();
    await fixture.whenStable();
    notifyVisibility(true);
    await waitForRequest(3);
    expect(requestedPages[2]).toBe(2);
    expect(component.listOfProblems().map((entry: SpraywallProblemDto): string => entry.id)).toEqual(['filtered']);
  });

  it('reports failed requests and retries the same page when the placeholder becomes visible again', async (): Promise<void> => {
    await renderPage(0, 2);
    notifyVisibility(true);
    await waitForRequest(2);
    responses[1].error(new Error('Network failure'));
    expect(toastService.showDanger).toHaveBeenCalled();

    notifyVisibility(false);
    notifyVisibility(true);
    await waitForRequest(3);
    expect(requestedPages.slice(1)).toEqual([2, 2]);
    responses[2].next({ currentPage: 2, totalCount: 2, problems: [problem('2')] });
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.listOfProblems().length).toBe(2);
  });
});

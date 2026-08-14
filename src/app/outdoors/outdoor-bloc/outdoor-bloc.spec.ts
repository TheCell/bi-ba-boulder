import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Data, ParamMap, Router } from '@angular/router';
import { BlocDto, LineDto, LinesService } from '@api-net/index';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { ColorService } from '../../core/util-services/color.service';
import { ModalService } from '../../core/modal/modal.service';
import { ToastService } from '../../core/toast-container/toast.service';
import { OutdoorBloc } from './outdoor-bloc';

interface ActivatedRouteStub {
  readonly snapshot: {
    readonly data: Data;
    readonly paramMap: ParamMap;
  };
  readonly data: Observable<Data>;
  readonly queryParamMap: Observable<ParamMap>;
}

describe('OutdoorBloc', (): void => {
  const blocA: BlocDto = { id: 'bloc-a', name: 'Bloc A' };
  const blocB: BlocDto = { id: 'bloc-b', name: 'Bloc B' };
  const blocC: BlocDto = { id: 'bloc-c', name: 'Bloc C' };
  const blocs: BlocDto[] = [blocA, blocB, blocC];
  const routeData = new BehaviorSubject<Data>({ bloc: blocA, blocs });
  const route: ActivatedRouteStub = {
    snapshot: {
      data: routeData.value,
      paramMap: convertToParamMap({ outdoorAreaId: 'area-1', sectorId: 'sector-1', id: blocA.id })
    },
    data: routeData.asObservable(),
    queryParamMap: of(convertToParamMap({}))
  };
  const getLinesByBlocId = jasmine.createSpy<(blocId: string) => Observable<LineDto[]>>('getLinesByBlocId');
  const linesService = { getLinesByBlocId };
  const boulderLoaderService = jasmine.createSpyObj<BoulderLoaderService>('BoulderLoaderService', [
    'getUrls',
    'getNextResolution',
    'loadBoulder'
  ]);

  let fixture: ComponentFixture<OutdoorBloc>;
  let component: OutdoorBloc;

  beforeEach(async (): Promise<void> => {
    routeData.next({ bloc: blocA, blocs });
    getLinesByBlocId.and.returnValue(of([]));
    boulderLoaderService.getUrls.and.returnValue({ urls: [], blocIds: [], currentResolution: undefined });

    await TestBed.configureTestingModule({
      imports: [OutdoorBloc],
      providers: [
        { provide: ActivatedRoute, useValue: route },
        { provide: LinesService, useValue: linesService },
        { provide: BoulderLoaderService, useValue: boulderLoaderService },
        { provide: ToastService, useValue: {} },
        { provide: ColorService, useValue: {} },
        { provide: ModalService, useValue: {} },
        { provide: Router, useValue: {} }
      ]
    })
      .overrideComponent(OutdoorBloc, { set: { imports: [], template: '' } })
      .compileComponents();

    fixture = TestBed.createComponent(OutdoorBloc);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('cycles from the first bloc to the last and second blocs', (): void => {
    expect(component.previousBloc).toBe(blocC);
    expect(component.nextBloc).toBe(blocB);
    expect(component.blocRouterLink(blocC.id)).toEqual([
      '/',
      'outdoor-area',
      'area-1',
      'sector',
      'sector-1',
      'bloc',
      blocC.id
    ]);
  });

  it('refreshes the bloc and sibling links when resolved data changes', (): void => {
    routeData.next({ bloc: blocB, blocs });

    expect(component.bloc).toBe(blocB);
    expect(component.previousBloc).toBe(blocA);
    expect(component.nextBloc).toBe(blocC);
    expect(getLinesByBlocId).toHaveBeenCalledWith(blocB.id);
  });
});

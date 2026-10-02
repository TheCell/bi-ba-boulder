import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { BlocDto, BlocsService, SectorDto, SectorsService } from '@api-net/index';
import { of } from 'rxjs';
import '../../extensions/string.extensions';
import { ToastService } from '../../core/toast-container/toast.service';
import { BlocsAdmin } from './blocs-admin';

describe('BlocsAdmin', (): void => {
  let fixture: ComponentFixture<BlocsAdmin>;
  let component: BlocsAdmin;
  let blocsService: { getBlocsBySectorId: jasmine.Spy; getBlocsWithoutSector: jasmine.Spy; deleteBloc: jasmine.Spy };
  let sectorsService: { getSectors: jasmine.Spy };

  const sectors: SectorDto[] = [
    { id: 's2', name: 'The Shield', version: 1, isPublic: true },
    { id: 's1', name: 'Left Side', version: 1, isPublic: false }
  ];
  const blocs: BlocDto[] = [
    { id: '1', name: 'Dreamtime', version: 1, sectorId: 's1' },
    { id: '2', name: 'Big Paw', version: 1, sectorId: 's1' }
  ];
  const unassignedBlocs: BlocDto[] = [{ id: '3', name: 'New bloc', version: 1, sectorId: null }];

  beforeEach(async (): Promise<void> => {
    blocsService = {
      getBlocsBySectorId: jasmine.createSpy('getBlocsBySectorId').and.returnValue(of(blocs)),
      getBlocsWithoutSector: jasmine.createSpy('getBlocsWithoutSector').and.returnValue(of(unassignedBlocs)),
      deleteBloc: jasmine.createSpy('deleteBloc')
    };
    sectorsService = {
      getSectors: jasmine.createSpy('getSectors').and.returnValue(of(sectors))
    };
    const toastService = jasmine.createSpyObj<ToastService>('ToastService', ['showDanger', 'showSuccess']);

    await TestBed.configureTestingModule({
      imports: [BlocsAdmin],
      providers: [
        provideRouter([]),
        { provide: BlocsService, useValue: blocsService },
        { provide: SectorsService, useValue: sectorsService },
        { provide: ToastService, useValue: toastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BlocsAdmin);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach((): void => {
    fixture.destroy();
  });

  it('selects the first sector alphabetically and loads its blocs', (): void => {
    expect(component.selectedSectorId()).toBe('s1');
    expect(blocsService.getBlocsBySectorId).toHaveBeenCalledWith('s1');
    expect(component.blocs()).toEqual(blocs);
  });

  it('filters blocs by name', (): void => {
    component.onSearch({ target: { value: 'Paw' } } as unknown as Event);

    expect(component.filteredBlocs()).toEqual([blocs[1]]);
  });

  it('loads blocs of the newly selected sector', (): void => {
    component.onSectorChange({ target: { value: 's2' } } as unknown as Event);

    expect(component.selectedSectorId()).toBe('s2');
    expect(blocsService.getBlocsBySectorId).toHaveBeenCalledWith('s2');
  });

  it('removes a deleted bloc from the list', (): void => {
    component.onDeleteClosed({ closeType: 0, data: blocs[0] });

    expect(component.blocs()).toEqual([blocs[1]]);
  });

  it('loads unassigned blocs and preserves the filter in the URL', (): void => {
    const navigate: jasmine.Spy = spyOn(TestBed.inject(Router), 'navigate').and.resolveTo(true);

    component.onSectorChange({ target: { value: '' } } as unknown as Event);

    expect(component.selectedSectorId()).toBeNull();
    expect(blocsService.getBlocsWithoutSector).toHaveBeenCalled();
    expect(component.blocs()).toEqual(unassignedBlocs);
    expect(navigate).toHaveBeenCalledWith([], jasmine.objectContaining({ queryParams: { sectorId: 'unassigned' } }));
  });

  it('restores the unassigned filter from the URL', (): void => {
    spyOn(TestBed.inject(ActivatedRoute).snapshot.queryParamMap, 'get').and.returnValue('unassigned');

    component.ngOnInit();

    expect(component.selectedSectorId()).toBeNull();
    expect(component.blocs()).toEqual(unassignedBlocs);
  });

  it('loads unassigned blocs and allows creation when no sectors exist', (): void => {
    sectorsService.getSectors.and.returnValue(of([]));

    component.ngOnInit();
    fixture.detectChanges();

    expect(component.selectedSectorId()).toBeNull();
    expect(component.blocs()).toEqual(unassignedBlocs);
    expect(component.isLoading()).toBeFalse();
    const createButton: HTMLButtonElement = fixture.nativeElement.querySelector('.page-header button');
    expect(createButton.disabled).toBeFalse();
  });

  it('reloads unassigned blocs after saving', (): void => {
    component.onSectorChange({ target: { value: '' } } as unknown as Event);
    blocsService.getBlocsWithoutSector.calls.reset();

    component.onEditorClosed({ closeType: 0, data: unassignedBlocs[0] });

    expect(blocsService.getBlocsWithoutSector).toHaveBeenCalledTimes(1);
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { BlocDto, BlocsService, SectorDto, SectorsService } from '@api-net/index';
import { of } from 'rxjs';
import { ToastService } from '../../core/toast-container/toast.service';
import { BlocsAdmin } from './blocs-admin';

describe('BlocsAdmin', (): void => {
  let fixture: ComponentFixture<BlocsAdmin>;
  let component: BlocsAdmin;
  let blocsService: { getBlocsBySectorId: jasmine.Spy; deleteBloc: jasmine.Spy };

  const sectors: SectorDto[] = [
    { id: 's2', name: 'The Shield', version: 1, isPublic: true },
    { id: 's1', name: 'Left Side', version: 1, isPublic: false }
  ];
  const blocs: BlocDto[] = [
    { id: '1', name: 'Dreamtime', version: 1, sectorId: 's1' },
    { id: '2', name: 'Big Paw', version: 1, sectorId: 's1' }
  ];

  beforeEach(async (): Promise<void> => {
    blocsService = {
      getBlocsBySectorId: jasmine.createSpy('getBlocsBySectorId').and.returnValue(of(blocs)),
      deleteBloc: jasmine.createSpy('deleteBloc')
    };
    const sectorsService = {
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
});

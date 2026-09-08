import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SectorDto, SectorsService } from '@api-net/index';
import { of } from 'rxjs';
import { ToastService } from '../../core/toast-container/toast.service';
import { SectorsAdmin } from './sectors-admin';

describe('SectorsAdmin', (): void => {
  let fixture: ComponentFixture<SectorsAdmin>;
  let component: SectorsAdmin;

  const sectors: SectorDto[] = [
    { id: '1', name: 'The Shield', version: 1, isPublic: true },
    { id: '2', name: 'Left Side', version: 1, isPublic: false }
  ];

  beforeEach(async (): Promise<void> => {
    const sectorsService = {
      getSectors: jasmine.createSpy('getSectors').and.returnValue(of(sectors)),
      deleteSector: jasmine.createSpy('deleteSector')
    };
    const toastService = jasmine.createSpyObj<ToastService>('ToastService', ['showDanger', 'showSuccess']);

    await TestBed.configureTestingModule({
      imports: [SectorsAdmin],
      providers: [
        { provide: SectorsService, useValue: sectorsService },
        { provide: ToastService, useValue: toastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SectorsAdmin);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach((): void => {
    fixture.destroy();
  });

  it('loads sectors on init', (): void => {
    expect(component.sectors()).toEqual(sectors);
  });

  it('filters sectors by name', (): void => {
    component.onSearch({ target: { value: 'Shield' } } as unknown as Event);

    expect(component.filteredSectors()).toEqual([sectors[0]]);
  });
});

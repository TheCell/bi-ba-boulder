import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BoulderGymDto, BoulderGymService } from '@api-net/index';
import { of } from 'rxjs';
import { ToastService } from '../../core/toast-container/toast.service';
import { BoulderGymsAdmin } from './boulder-gyms-admin';

describe('BoulderGymsAdmin', (): void => {
  let fixture: ComponentFixture<BoulderGymsAdmin>;
  let component: BoulderGymsAdmin;

  const boulderGyms: BoulderGymDto[] = [
    { id: '1', name: 'Bimano', version: 1 },
    { id: '2', name: 'Magnet', version: 1 }
  ];

  beforeEach(async (): Promise<void> => {
    const boulderGymService = {
      getBoulderGyms: jasmine.createSpy('getBoulderGyms').and.returnValue(of(boulderGyms)),
      deleteBoulderGym: jasmine.createSpy('deleteBoulderGym')
    };
    const toastService = jasmine.createSpyObj<ToastService>('ToastService', ['showDanger', 'showSuccess']);

    await TestBed.configureTestingModule({
      imports: [BoulderGymsAdmin],
      providers: [
        { provide: BoulderGymService, useValue: boulderGymService },
        { provide: ToastService, useValue: toastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BoulderGymsAdmin);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach((): void => {
    fixture.destroy();
  });

  it('loads boulder gyms on init', (): void => {
    expect(component.boulderGyms()).toEqual(boulderGyms);
  });

  it('filters boulder gyms by name', (): void => {
    component.onSearch({ target: { value: 'Bimano' } } as unknown as Event);

    expect(component.filteredBoulderGyms()).toEqual([boulderGyms[0]]);
  });
});

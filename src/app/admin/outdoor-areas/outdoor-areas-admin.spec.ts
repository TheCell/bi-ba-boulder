import { ComponentFixture, TestBed } from '@angular/core/testing';
import { OutdoorAreaDto, OutdoorAreasService } from '@api-net/index';
import { of } from 'rxjs';
import { ToastService } from '../../core/toast-container/toast.service';
import { OutdoorAreasAdmin } from './outdoor-areas-admin';

describe('OutdoorAreasAdmin', (): void => {
  let fixture: ComponentFixture<OutdoorAreasAdmin>;
  let component: OutdoorAreasAdmin;

  const outdoorAreas: OutdoorAreaDto[] = [
    { id: '1', name: 'Magic Wood', version: 1 },
    { id: '2', name: 'Chironico', version: 1 }
  ];

  beforeEach(async (): Promise<void> => {
    const outdoorAreasService = {
      getOutdoorAreas: jasmine.createSpy('getOutdoorAreas').and.returnValue(of(outdoorAreas)),
      deleteOutdoorArea: jasmine.createSpy('deleteOutdoorArea')
    };
    const toastService = jasmine.createSpyObj<ToastService>('ToastService', ['showDanger', 'showSuccess']);

    await TestBed.configureTestingModule({
      imports: [OutdoorAreasAdmin],
      providers: [
        { provide: OutdoorAreasService, useValue: outdoorAreasService },
        { provide: ToastService, useValue: toastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OutdoorAreasAdmin);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach((): void => {
    fixture.destroy();
  });

  it('loads outdoor areas on init', (): void => {
    expect(component.outdoorAreas()).toEqual(outdoorAreas);
  });

  it('filters outdoor areas by name', (): void => {
    component.onSearch({ target: { value: 'Magic' } } as unknown as Event);

    expect(component.filteredOutdoorAreas()).toEqual([outdoorAreas[0]]);
  });
});

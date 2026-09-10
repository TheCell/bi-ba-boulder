import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MediasService, UriAliasAdministrationDto } from '@api-net/index';
import { of } from 'rxjs';
import { ToastService } from '../../core/toast-container/toast.service';
import { UriAliases } from './uri-aliases';

describe('UriAliases', (): void => {
  let fixture: ComponentFixture<UriAliases>;
  let component: UriAliases;

  const uriAliases: UriAliasAdministrationDto[] = [
    { id: '1', alias: 'main-gym', typeId: 1, targetId: 'gym', targetName: 'Bimano', version: 1 },
    { id: '2', alias: 'magic-wood', typeId: 0, targetId: 'area', targetName: 'Magic Wood', version: 1 }
  ];

  beforeEach(async (): Promise<void> => {
    const mediasService = {
      getUriAliases: jasmine.createSpy('getUriAliases').and.returnValue(of(uriAliases)),
      deleteUriAlias: jasmine.createSpy('deleteUriAlias')
    };
    const toastService = jasmine.createSpyObj<ToastService>('ToastService', ['showDanger', 'showSuccess']);

    await TestBed.configureTestingModule({
      imports: [UriAliases],
      providers: [
        { provide: MediasService, useValue: mediasService },
        { provide: ToastService, useValue: toastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UriAliases);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach((): void => {
    fixture.destroy();
  });

  it('filters aliases by target name', (): void => {
    component.onSearch({ target: { value: 'Bimano' } } as unknown as Event);

    expect(component.filteredUriAliases()).toEqual([uriAliases[0]]);
  });

  it('filters aliases by type', (): void => {
    component.onTypeFilter({ target: { value: '0' } } as unknown as Event);

    expect(component.filteredUriAliases()).toEqual([uriAliases[1]]);
  });

  it('returns readable type names', (): void => {
    expect(component.getTypeName(1)).toBe('Boulder gym');
    expect(component.getTypeName(0)).toBe('Outdoor area');
  });
});

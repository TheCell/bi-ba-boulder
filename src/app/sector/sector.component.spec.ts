import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router';
import { SectorComponent } from './sector.component';
import { BlocDto } from '@api-net/index';

describe('SectorComponent', () => {
  let component: SectorComponent;
  let fixture: ComponentFixture<SectorComponent>;
  let mockActivatedRoute: { snapshot: Partial<ActivatedRouteSnapshot> };

  const mockBlocs: BlocDto[] = [
    {
      id: '1',
      name: 'Test Bloc 1',
      description: 'First test bloc',
      blocLowRes: null,
      blocMedRes: null,
      blocHighRes: null
    },
    {
      id: '2',
      name: 'Test Bloc 2',
      description: 'Second test bloc',
      blocLowRes: null,
      blocMedRes: null,
      blocHighRes: null
    }
  ];

  beforeEach(async () => {
    mockActivatedRoute = {
      snapshot: {
        data: { blocs: mockBlocs }
      }
    };

    await TestBed.configureTestingModule({
      imports: [SectorComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load blocs from route snapshot', () => {
    expect(component.blocs).toEqual(mockBlocs);
    expect(component.blocs.length).toBe(2);
  });

  it('should render bloc links in the template', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const links = compiled.querySelectorAll('a');
    
    expect(links.length).toBe(2);
    expect(links[0].textContent?.trim()).toBe('Test Bloc 1');
    expect(links[1].textContent?.trim()).toBe('Test Bloc 2');
  });

  it('should handle empty blocs array', () => {
    mockActivatedRoute.snapshot!.data = { blocs: [] };
    
    const newFixture = TestBed.createComponent(SectorComponent);
    newFixture.detectChanges();
    
    const compiled = newFixture.nativeElement as HTMLElement;
    const links = compiled.querySelectorAll('a');
    
    expect(links.length).toBe(0);
  });
});

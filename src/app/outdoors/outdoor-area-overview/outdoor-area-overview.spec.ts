import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { OutdoorAreaDto } from '@api-net/index';
import { OutdoorAreaOverview } from './outdoor-area-overview';

describe('OutdoorAreaOverview', (): void => {
  const outdoorArea: OutdoorAreaDto = {
    id: 'area-1',
    name: 'Magic Wood',
    description: 'A forest bouldering area.',
    importantInfo: 'Park only in designated spaces.',
    images: [
      { resourceType: 0, uri: 'https://example.com/area.jpg' },
      { resourceType: 1, uri: 'https://example.com/area.mp4' }
    ],
    sectors: [
      {
        id: 'sector-1',
        name: 'Riverbed',
        previewImageUri: 'https://example.com/sector.jpg'
      }
    ]
  };

  let fixture: ComponentFixture<OutdoorAreaOverview>;
  let element: HTMLElement;

  beforeEach(async (): Promise<void> => {
    await TestBed.configureTestingModule({
      imports: [OutdoorAreaOverview],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { data: { outdoorArea } } }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OutdoorAreaOverview);
    fixture.detectChanges();
    element = fixture.nativeElement as HTMLElement;
  });

  it('renders the outdoor area content and image resources', (): void => {
    expect(element.querySelector('h1')?.textContent).toContain('Magic Wood');
    expect(element.querySelector('.important-info')?.textContent).toContain('Park only in designated spaces.');
    expect(element.querySelector('.description')?.textContent).toContain('A forest bouldering area.');
    expect(element.querySelectorAll('.area-image').length).toBe(1);
  });

  it('links every sector without a nested scrolling container', (): void => {
    const sectorLink: HTMLAnchorElement | null = element.querySelector<HTMLAnchorElement>('.sector-tile');

    expect(sectorLink?.textContent).toContain('Riverbed');
    expect(sectorLink?.getAttribute('href')).toBe('/sectors/sector-1');
  });
});

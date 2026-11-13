import { Component, inject } from '@angular/core';
import { AuthSessionStateService } from '../auth/auth-session-state.service';

interface ReleaseHighlight {
  text: string;
  isOutdoorRelated: boolean;
}

interface ReleaseNote {
  prNumber: number;
  releasedOn: string;
  headline: string;
  highlights: ReleaseHighlight[];
}

interface VisibleReleaseNote {
  prNumber: number;
  releasedOn: string;
  headline: string;
  highlights: string[];
}

@Component({
  selector: 'app-changelog',
  imports: [],
  templateUrl: './changelog.component.html',
  styleUrl: './changelog.component.scss'
})
export class ChangelogComponent {
  private readonly authSessionStateService = inject(AuthSessionStateService);

  public readonly releaseNotes: ReleaseNote[] = [
    {
      prNumber: 121,
      releasedOn: '2026-11-13',
      headline: 'Updated Filtering and UX',
      highlights: [
        {
          text: 'Updated filtering and improved user experience.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 120,
      releasedOn: '2026-11-06',
      headline: 'Database-backed spraywall loading',
      highlights: [
        {
          text: 'Added a spraywall API and database model, and updated spraywall views, editors, and loaders to use database-backed data.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 119,
      releasedOn: '2026-10-16',
      headline: 'Deployment workflow fix',
      highlights: [
        {
          text: 'Fixed the backend publish output path used by the build and deployment workflows.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 118,
      releasedOn: '2026-10-16',
      headline: '.NET build and test workflow updates',
      highlights: [
        {
          text: 'Updated the .NET SDK and dependencies, and refreshed CI build, test, and code coverage workflows.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 117,
      releasedOn: '2026-10-09',
      headline: 'Image URI handling and backend test refinements',
      highlights: [
        {
          text: 'Stopped rejecting non-relative image URIs and updated the related backend tests.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 116,
      releasedOn: '2026-10-09',
      headline: 'Package script configuration',
      highlights: [
        {
          text: 'Allowed required install scripts for OpenAPI code generation and esbuild dependencies.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 115,
      releasedOn: '2026-10-09',
      headline: 'Outdoor bloc swipe navigation',
      highlights: [
        {
          text: 'Added touch swipe navigation with a visual indicator for moving between outdoor blocs.',
          isOutdoorRelated: true
        }
      ]
    },
    {
      prNumber: 114,
      releasedOn: '2026-10-02',
      headline: 'Outdoor bloc administration and area visibility',
      highlights: [
        {
          text: 'Added create, edit, and delete tools for outdoor blocs, along with public visibility controls for outdoor areas.',
          isOutdoorRelated: true
        },
        {
          text: 'Improved loading feedback and the spraywall launch button.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 112,
      releasedOn: '2026-09-10',
      headline: 'Media content management and gym/outdoor area admin features',
      highlights: [
        {
          text: 'Added media content management with image modal and endpoints for uploading and managing media across gyms and outdoor areas.',
          isOutdoorRelated: true
        },
        {
          text: 'Implemented URI alias management system for custom friendly URLs to gyms and outdoor areas.',
          isOutdoorRelated: false
        },
        {
          text: 'Enhanced UI with improved modal handling, unified scrollbar design, and prevented modal interactions on mobile devices.',
          isOutdoorRelated: false
        },
        {
          text: 'Reorganized project structure and added session keep-alive functionality for better stability.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 111,
      releasedOn: '2026-08-14',
      headline: 'Outdoor area release, access control, and scene markings',
      highlights: [
        {
          text: 'Released the outdoor area experience with areas, sectors, multiple bloc parts, previews, and public content access.',
          isOutdoorRelated: true
        },
        {
          text: 'Added sector access control so private outdoor content can be shared with authorized users.',
          isOutdoorRelated: true
        },
        {
          text: 'Added scene markings for route starts, tops, and off-line holds with editable sphere and box shapes.',
          isOutdoorRelated: true
        },
        {
          text: 'Improved outdoor rendering with level-of-detail models, line colors, line focus, and easier line selection.',
          isOutdoorRelated: true
        },
        {
          text: 'Added IndexedDB model caching and continued the frontend migration to zoneless Angular operation.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 110,
      releasedOn: '2026-08-07',
      headline: 'Scene markings, line selection UX, and zoneless groundwork',
      highlights: [
        {
          text: 'Introduced scene marking foundations across backend models and generated frontend API types.',
          isOutdoorRelated: true
        },
        {
          text: 'Improved outdoor line selection with tube raycasting, line focus behavior, and line color feedback.',
          isOutdoorRelated: true
        },
        {
          text: 'Moved the app setup toward zoneless operation and refactored renderer event handling for better stability.',
          isOutdoorRelated: false
        },
        {
          text: 'Cleaned up renderer/shader structure and refreshed changelog presentation styling.',
          isOutdoorRelated: false
        }
      ]
    },
    {
      prNumber: 108,
      releasedOn: '2026-07-31',
      headline: 'Bloc caching, navigation icons, and outdoor editor improvements',
      highlights: [
        {
          text: 'Cached bloc 3D models per resolution level — revisiting a bloc skips re-downloading already loaded quality levels.',
          isOutdoorRelated: true
        },
        {
          text: 'Cached spraywall 3D models so navigating between spraywall views and editor avoids redundant downloads.',
          isOutdoorRelated: false
        },
        {
          text: 'Added icons to the navigation bar for login, logout, sectors, and changelog.',
          isOutdoorRelated: false
        },
        {
          text: 'Fixed raycasting in the outdoor editor renderer.',
          isOutdoorRelated: true
        }
      ]
    },
    {
      prNumber: 107,
      releasedOn: '2026-07-24',
      headline: 'Route sharing and outdoor line usability improvements',
      highlights: [
        {
          text: 'Added route sharing via QR code and direct link sharing.',
          isOutdoorRelated: false
        },
        {
          text: 'Improved line interaction with robust clicking and explicit line focus behavior.',
          isOutdoorRelated: true
        },
        {
          text: 'Updated line editing flow and controller handling for outdoor routes.',
          isOutdoorRelated: true
        },
        {
          text: 'Moved overlays and refined sorting plus list styling in outdoor bloc views.',
          isOutdoorRelated: true
        }
      ]
    },
    {
      prNumber: 106,
      releasedOn: '2026-07-17',
      headline: 'Line interaction polish',
      highlights: [
        { text: 'Fixed line colors.', isOutdoorRelated: true },
        { text: 'Limited line interactions to left-click input for predictable behavior.', isOutdoorRelated: true }
      ]
    },
    {
      prNumber: 105,
      releasedOn: '2026-07-17',
      headline: 'Outdoor line list and interaction updates',
      highlights: [
        { text: 'Added clickable line interactions and a dedicated lines list.', isOutdoorRelated: true },
        { text: 'Fixed outdoor line handling.', isOutdoorRelated: true },
        { text: 'Added copyright script and refreshed readme content.', isOutdoorRelated: false }
      ]
    },
    {
      prNumber: 104,
      releasedOn: '2026-07-10',
      headline: 'Outdoor editor and line CRUD foundation',
      highlights: [
        { text: 'Implemented line CRUD backend and added corresponding migration.', isOutdoorRelated: true },
        {
          text: 'Advanced outdoor editor with working rendering, dragging, and camera positioning.',
          isOutdoorRelated: true
        },
        { text: 'Added route materials and progressed line workflow integration.', isOutdoorRelated: true }
      ]
    },
    {
      prNumber: 103,
      releasedOn: '2026-07-03',
      headline: 'Spraywall reset and SQL script maintenance',
      highlights: [
        { text: 'Reworked obsolete methods.', isOutdoorRelated: false },
        { text: 'Upgraded and fixed reset behavior in the spraywall editor.', isOutdoorRelated: false },
        { text: 'Fixed SQL and updated scripts plus documentation.', isOutdoorRelated: false }
      ]
    },
    {
      prNumber: 94,
      releasedOn: '2026-06-28',
      headline: 'Fileshare setup and outdoor refactoring groundwork',
      highlights: [
        { text: 'Set up local fileshare placeholders and updated related structure.', isOutdoorRelated: false },
        { text: 'Continued outdoor blocs and boulders work-in-progress stream.', isOutdoorRelated: true },
        { text: 'Removed older components and cleaned up obsolete files.', isOutdoorRelated: false }
      ]
    }
  ];

  public get visibleReleaseNotes(): VisibleReleaseNote[] {
    return this.releaseNotes
      .map((releaseNote) => {
        const visibleHighlights = releaseNote.highlights;
        // const visibleHighlights = this.authSessionStateService.isAdmin()
        //   ? releaseNote.highlights
        //   : releaseNote.highlights.filter((highlight) => !highlight.isOutdoorRelated);

        return {
          prNumber: releaseNote.prNumber,
          releasedOn: releaseNote.releasedOn,
          headline: releaseNote.headline,
          highlights: visibleHighlights.map((highlight) => highlight.text)
        };
      })
      .filter((releaseNote) => releaseNote.highlights.length > 0);
  }
}

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
        const visibleHighlights = this.authSessionStateService.isAdmin()
          ? releaseNote.highlights
          : releaseNote.highlights.filter((highlight) => !highlight.isOutdoorRelated);

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

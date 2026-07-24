import { Component, inject, input } from '@angular/core';
import { Icon } from '../../core/icon/icon';
import { ToastService } from '../../core/toast-container/toast.service';

@Component({
  selector: 'app-socials-overlay',
  imports: [Icon],
  templateUrl: './socials-overlay.html',
  styleUrl: './socials-overlay.scss'
})
export class SocialsOverlay {
  private toastService = inject(ToastService);

  public routeUrl = input<string | undefined>();

  public async shareUrl(): Promise<void> {
    const routeUrl = this.routeUrl();
    if (!routeUrl) {
      return;
    }

    let wasShared = false;
    if (this.isMobileDevice() && typeof navigator.share === 'function') {
      try {
        await navigator.share({ url: routeUrl });
        wasShared = true;
      } catch (_error: unknown) {
        wasShared = false;
      }
    }

    if (wasShared) {
      return;
    }

    if (navigator.clipboard?.writeText) {
      try {
        await navigator.clipboard.writeText(routeUrl);
        this.toastService.showInfo('Copied to clipboard', 'The URL has been copied to your clipboard.');
      } catch (_error: unknown) {
        window.prompt('Copy this URL', routeUrl);
      }
      return;
    }

    window.prompt('Copy this URL', routeUrl);
  }

  public showQrCode(): void {
    console.log('todo');
  }

  private isMobileDevice(): boolean {
    const isCoarsePointer = window.matchMedia('(pointer: coarse)').matches;
    const hasTouch = navigator.maxTouchPoints > 0;
    return isCoarsePointer || hasTouch;
  }
}


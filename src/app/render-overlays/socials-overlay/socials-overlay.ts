import { Component, inject, input, ViewChild } from '@angular/core';
import { Icon } from '../../core/icon/icon';
import { ToastService } from '../../core/toast-container/toast.service';
import { Modal } from '../../core/modal/modal/modal';
import { SocialsDialogData } from './socials-dialog/socials-dialog-data';
import { ModalService } from '../../core/modal/modal.service';
import { SocialsDialog } from './socials-dialog/socials-dialog';

@Component({
  selector: 'app-socials-overlay',
  imports: [Icon, Modal],
  templateUrl: './socials-overlay.html',
  styleUrl: './socials-overlay.scss'
})
export class SocialsOverlay {
  @ViewChild('modal') private modal!: Modal;
  private toastService = inject(ToastService);
  private modalService = inject(ModalService);

  public routeUrl = input<string | undefined>();

  public async shareUrl(): Promise<void> {
    const routeUrl = this.routeUrl();
    if (!routeUrl) {
      return;
    }

    let wasShared = false;
    if (this.isMobileDevice() && typeof navigator.share === 'function') {
      await navigator.share({ url: routeUrl });
      wasShared = true;
      // try {
      //   wasShared = true;
      // } catch (_error: unknown) {
      //   wasShared = false;
      // }
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
    const routeUrl = this.routeUrl();
    if (routeUrl === undefined) {
      return;
    }

    const socialsDialogData: SocialsDialogData = {
      url: routeUrl
    };
    const modal = this.modalService.open(this.modal.id, SocialsDialog);
    if (modal && modal.initialize) {
      modal.initialize(socialsDialogData);
    }
  }

  private isMobileDevice(): boolean {
    const isCoarsePointer = window.matchMedia('(pointer: coarse)').matches;
    const hasTouch = navigator.maxTouchPoints > 0;
    return isCoarsePointer || hasTouch;
  }
}


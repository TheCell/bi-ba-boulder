import { Component, ElementRef, output, ViewChild } from '@angular/core';
import { IModal } from '../../../core/modal/modal/modal.interface';
import { CloseModalEvent } from '../../../core/modal/modal/close-modal-event';
import { SocialsDialogData } from './socials-dialog-data';
import QRCode from 'qrcode-generator';

@Component({
  selector: 'app-socials-dialog',
  imports: [],
  templateUrl: './socials-dialog.html',
  styleUrl: './socials-dialog.scss'
})
export class SocialsDialog implements IModal {
  @ViewChild('qrCodeCanvas', { static: true }) private qrCodeCanvas!: ElementRef<HTMLCanvasElement>;
  public closeModal = output<CloseModalEvent>();

  public canCloseWithoutPermission = true;

  public initialize(data: SocialsDialogData): void {
    const cellSize = 6;
    const margin = cellSize * 2;
    const qrCode = QRCode(8, 'L');
    qrCode.addData(data.url, 'Byte');
    qrCode.make();

    const canvas = this.qrCodeCanvas.nativeElement;

    const context = canvas.getContext('2d');
    if (context) {
      context.fillStyle = '#1f1f1f';
      context.fillRect(0, 0, canvas.width, canvas.height);
      const size = qrCode.getModuleCount() * cellSize + margin * 2;
      canvas.width = size;
      canvas.height = size;
      context.fillStyle = '#cccccc';
      for (let row = 0; row < qrCode.getModuleCount(); row += 1) {
        for (let col = 0; col < qrCode.getModuleCount(); col += 1) {
          if (qrCode.isDark(row, col)) {
            context.fillRect(col * cellSize + margin, row * cellSize + margin, cellSize, cellSize);
          }
        }
      }
    }
  }
}


import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';

@Component({
  selector: 'app-boulder',
  standalone: true,
  imports: [
    CommonModule 
  ],
  templateUrl: './boulder.component.html',
  styleUrl: './boulder.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoulderComponent {
  public title: string;
  public boulderImageUrl: SafeStyle;
  public image: string;

  public constructor(private domSanitizer: DomSanitizer) {
    this.title = 'test';
    this.image = '/test-images/bimano-image.webp';
    this.boulderImageUrl = this.domSanitizer.bypassSecurityTrustStyle('url(/test-images/bimano-image.webp)');
  }
}

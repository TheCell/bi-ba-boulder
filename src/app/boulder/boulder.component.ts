import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';
import { BoulderRenderComponent } from "../boulder-render/boulder-render.component";

@Component({
  selector: 'app-boulder',
  standalone: true,
  imports: [
    CommonModule,
    BoulderRenderComponent,
    BoulderRenderComponent
],
  templateUrl: './boulder.component.html',
  styleUrl: './boulder.component.scss'
})
export class BoulderComponent {
  public title: string;
  public boulderImageUrl: SafeStyle;
  public image: string;
  public modelLoaded = false;

  public constructor(private domSanitizer: DomSanitizer) {
    this.title = 'test';
    this.image = '/test-images/bimano-image.webp';
    this.boulderImageUrl = this.domSanitizer.bypassSecurityTrustStyle('url(/test-images/bimano-image.webp)');

    setTimeout(() => {
      // faking for now
      this.modelLoaded = true;
    }, 2000);
  }
}

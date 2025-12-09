
import { Component } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';
import { BoulderHardcodedRenderComponent } from "../boulder-hardcoded-render/boulder-hardcoded-render.component";

@Component({
  selector: 'app-boulder-hardcoded',
  imports: [
    BoulderHardcodedRenderComponent
],
  templateUrl: './boulder-hardcoded.component.html',
  styleUrl: './boulder-hardcoded.component.scss'
})
export class BoulderHardcodedComponent {
  public title: string;
  public boulderImageUrl: SafeStyle;
  public image: string;
  public modelLoaded = false;

  public constructor(private domSanitizer: DomSanitizer) {
    this.title = 'test';
    this.image = './test-images/bimano-image.webp';
    this.boulderImageUrl = this.domSanitizer.bypassSecurityTrustStyle('url(./test-images/bimano-image.webp)');

    setTimeout(() => {
      // faking for now
      this.modelLoaded = true;
    }, 2000);
  }
}

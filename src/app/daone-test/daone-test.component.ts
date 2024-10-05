import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';
import { DaoneRenderTestComponent } from '../daone-render-test/daone-render-test.component';

@Component({
  selector: 'app-daone-test',
  standalone: true,
  imports: [
    CommonModule,
    DaoneRenderTestComponent
  ],
  templateUrl: './daone-test.component.html',
  styleUrl: './daone-test.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DaoneTestComponent {
  public title: string;
  public boulderImageUrl: SafeStyle;
  public image: string;
  public modelLoaded = false;

  public constructor(private domSanitizer: DomSanitizer) {
    this.title = 'test';
    this.image = './test-images/Bloc_5.jpg';
    this.boulderImageUrl = this.domSanitizer.bypassSecurityTrustStyle('url(./test-images/Bloc_5.jpg)');

    setTimeout(() => {
      // faking for now
      this.modelLoaded = true;
    }, 2000);
  }
}

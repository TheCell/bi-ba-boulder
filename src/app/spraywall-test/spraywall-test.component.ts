import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from '@angular/core';
import { BoulderRenderComponent } from '../boulder-render/boulder-render.component';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import { BoulderLine } from '../interfaces/boulder-line';

@Component({
  selector: 'app-spraywall-test',
  standalone: true,
  imports: [BoulderRenderComponent],
  templateUrl: './spraywall-test.component.html',
  styleUrl: './spraywall-test.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallTestComponent {
  public currentRawModel?: ArrayBuffer = undefined;
    public currentLines: BoulderLine[] = [];
  
  public constructor(
    private boulderLoaderService: BoulderLoaderService,
    private changeDetectorRef: ChangeDetectorRef) {
    const testBoulder = this.boulderLoaderService.loadTestSpraywall();
    testBoulder.subscribe({
      next: (data) => {
        // console.log('data', data);
        this.currentRawModel = data;
        this.changeDetectorRef.markForCheck();
        // this.addBoulderToScene(data);
      }
    });
  }
}

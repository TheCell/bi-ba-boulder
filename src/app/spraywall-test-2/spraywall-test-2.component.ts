import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from '@angular/core';
import { BoulderLoaderService } from '../background-loading/boulder-loader.service';
import { BoulderLine } from '../interfaces/boulder-line';
import { BoulderDebugRenderComponent } from '../boulder-debug-render/boulder-debug-render.component';

@Component({
  selector: 'app-spraywall-test-2',
  imports: [BoulderDebugRenderComponent],
  templateUrl: './spraywall-test-2.component.html',
  styleUrl: './spraywall-test-2.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SpraywallTest2Component {
  public currentRawModel?: ArrayBuffer = undefined;
    public currentLines: BoulderLine[] = [];
  
  public constructor(
    private boulderLoaderService: BoulderLoaderService,
    private changeDetectorRef: ChangeDetectorRef) {
    const testBoulder = this.boulderLoaderService.loadTestSpraywall2();
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

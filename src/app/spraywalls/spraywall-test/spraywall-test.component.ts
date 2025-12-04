import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject } from '@angular/core';
import { BoulderRenderComponent } from '../../renderer/boulder-render/boulder-render.component';
import { BoulderLoaderService } from '../../background-loading/boulder-loader.service';
import { BoulderLine } from '../../interfaces/boulder-line';

@Component({
  selector: 'app-spraywall-test',
  imports: [BoulderRenderComponent],
  templateUrl: './spraywall-test.component.html',
  styleUrl: './spraywall-test.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SpraywallTestComponent {
  private boulderLoaderService: BoulderLoaderService = inject(BoulderLoaderService);
  private changeDetectorRef: ChangeDetectorRef = inject(ChangeDetectorRef);
  public currentRawModel?: ArrayBuffer = undefined;
    public currentLines: BoulderLine[] = [];
  
  public constructor() {
    const testBoulder = this.boulderLoaderService.loadTestSpraywall();
    testBoulder.subscribe({
      next: (data: ArrayBuffer) => {
        // console.log('data', data);
        this.currentRawModel = data;
        this.changeDetectorRef.markForCheck();
        // this.addBoulderToScene(data);
      }
    });
  }
}

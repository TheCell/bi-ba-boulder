import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { SpraywallProblemDto } from '@api/index';

@Component({
  selector: 'app-spraywall-legend-item',
  imports: [],
  templateUrl: './spraywall-legend-item.html',
  styleUrl: './spraywall-legend-item.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallLegendItem {
  public problem = input.required<SpraywallProblemDto>();
  public isSelected = input<boolean>(false);
  public selected = output<SpraywallProblemDto>();

  public selectProblem() {
    this.selected.emit(this.problem());
  }
}

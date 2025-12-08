import { DatePipe, JsonPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { SpraywallProblemDto } from '@api/index';
import { FontGradePipePipe } from 'src/app/core/pipes/font-grade-pipe-pipe';

@Component({
  selector: 'app-spraywall-legend-item',
  imports: [FontGradePipePipe, DatePipe, JsonPipe],
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

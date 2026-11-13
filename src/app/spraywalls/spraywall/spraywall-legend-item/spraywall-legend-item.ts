import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { SpraywallProblemDto } from '@api-net/index';
import { FontGradePipePipe } from '../../../core/pipes/font-grade-pipe-pipe';
import { findSpraywallNameMatch } from '../spraywall-name-match';

@Component({
  selector: 'app-spraywall-legend-item',
  imports: [FontGradePipePipe, DatePipe],
  templateUrl: './spraywall-legend-item.html',
  styleUrl: './spraywall-legend-item.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SpraywallLegendItem {
  public problem = input.required<SpraywallProblemDto>();
  public isSelected = input<boolean>(false);
  public searchTerm = input<string>('');
  public selected = output<SpraywallProblemDto>();
  public nameMatch = computed(() => findSpraywallNameMatch(this.problem().name, this.searchTerm()));

  public selectProblem() {
    this.selected.emit(this.problem());
  }
}

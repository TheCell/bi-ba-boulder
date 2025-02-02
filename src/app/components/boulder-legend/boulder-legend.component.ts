import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { BoulderLine } from '../../interfaces/boulder-line';
import { FONT_GRADE } from '../../interfaces/font-grade';

@Component({
  selector: 'app-boulder-legend',
  imports: [
    CommonModule,
  ],
  templateUrl: './boulder-legend.component.html',
  styleUrl: './boulder-legend.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BoulderLegendComponent {
  public lines = input<BoulderLine[]>([]);
  public title = input<string>();
  public FONT_GRADE = FONT_GRADE;
}

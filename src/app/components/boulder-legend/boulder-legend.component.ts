import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { BoulderLine } from '../../api/interfaces/boulder-line';
import { FONT_GRADE } from '../../api/interfaces/font-grade';

@Component({
  selector: 'app-boulder-legend',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './boulder-legend.component.html',
  styleUrl: './boulder-legend.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoulderLegendComponent {
  public lines = input.required<BoulderLine[]>();
  public FONT_GRADE = FONT_GRADE;
}

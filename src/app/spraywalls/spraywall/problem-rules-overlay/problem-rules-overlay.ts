import { Component, inject, input } from '@angular/core';
import { SpraywallProblemDto } from '@api-net/index';
import { Icon } from '../../../core/icon/icon';
import { ToastService } from '../../../core/toast-container/toast.service';

type ProblemRuleType = 'circuit' | 'freeFeet' | 'noMatch';

@Component({
  selector: 'app-problem-rules-overlay',
  imports: [Icon],
  templateUrl: './problem-rules-overlay.html',
  styleUrl: './problem-rules-overlay.scss'
})
export class ProblemRulesOverlay {
  private toastService = inject(ToastService);
  public boulderProblem = input<SpraywallProblemDto>();

  public onExpandClick(problemRuleType: ProblemRuleType): void {
    if (problemRuleType === 'circuit') {
      this.toastService.showInfo(
        'Circuit',
        'A circuit is a sequence of moves that can be climbed endlessly without touching the ground.'
      );
    } else if (problemRuleType === 'freeFeet') {
      this.toastService.showInfo('Free Feet', 'Free feet means you can use your feet freely without restrictions.');
    } else if (problemRuleType === 'noMatch') {
      this.toastService.showInfo('No Match', 'No match means you cannot match certain holds.');
    }
  }
}


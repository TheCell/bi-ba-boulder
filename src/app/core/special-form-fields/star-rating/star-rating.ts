import { ChangeDetectionStrategy, Component, model, signal } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { Icon } from '../../icon/icon';

@Component({
  selector: 'app-star-rating',
  imports: [Icon],
  templateUrl: './star-rating.html',
  styleUrl: './star-rating.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(focusin)': 'isFocused.set(true)',
    '(focusout)': 'isFocused.set(false)'
  }
})
export class StarRating implements FormValueControl<number | undefined> {
  public ratingRange = Array.from({ length: 5 }, (_, i) => i + 1);
  public value = model<number | undefined>(undefined);
  public disabled = model<boolean>(false);
  public isFocused = signal<boolean>(false);
  // public focusedStar = signal<number | undefined>(undefined);

  public isFilled(star: number): boolean {
    return (this.value() ?? 0) >= star;
  }

  public setRating(star: number): void {
    if (this.disabled()) {
      return;
    }

    if (this.value() === star) {
      this.value.set(undefined);
    } else {
      this.value.set(star);
    }
  }
}

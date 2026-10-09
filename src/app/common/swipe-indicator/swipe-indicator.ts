import { Component, effect, input, output, signal, untracked } from '@angular/core';
import { SwipeEvent } from './swipe-event';
import { NgStyle, NgClass } from '@angular/common';

@Component({
  selector: 'app-swipe-indicator',
  imports: [NgStyle, NgClass],
  templateUrl: './swipe-indicator.html',
  styleUrl: './swipe-indicator.scss'
})
export class SwipeIndicator {
  public rightSide = input<boolean>(true);
  public swipeEvent = input<SwipeEvent | undefined>();
  public threshold = input<number>(200);
  public swiped = output<number>();

  public delta = signal<number>(0);
  public isRightToLeft = signal<boolean>(false);
  public hasReachedThreshold = signal<boolean>(false);
  public math = Math;

  private startX: number | undefined;

  public constructor() {
    effect((): void => {
      const swipeEvent: SwipeEvent | undefined = this.swipeEvent();
      untracked((): void => this.updateSwipe(swipeEvent));
    });
  }

  private updateSwipe(swipeEvent: SwipeEvent | undefined): void {
    this.hasReachedThreshold.set(Math.abs(this.delta()) >= this.threshold());

    if (swipeEvent !== undefined) {
      this.startX ??= swipeEvent.startX;
      this.delta.set(swipeEvent.endX - this.startX);
      this.isRightToLeft.set(this.delta() < 0);
      return;
    }

    if (this.startX === undefined) {
      return;
    }

    const delta: number = this.delta();
    this.startX = undefined;
    this.delta.set(0);
    this.hasReachedThreshold.set(false);

    if (Math.abs(delta) > this.threshold()) {
      this.swiped.emit(delta);
    }
  }
}


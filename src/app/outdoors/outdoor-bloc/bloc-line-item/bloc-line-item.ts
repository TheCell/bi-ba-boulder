import { Component, input, output } from '@angular/core';
import { LineDto } from '@api-net/model/models';
import { FontGradePipePipe } from '../../../core/pipes/font-grade-pipe-pipe';

@Component({
  selector: 'app-bloc-line-item',
  imports: [FontGradePipePipe],
  templateUrl: './bloc-line-item.html',
  styleUrl: './bloc-line-item.scss'
})
export class BlocLineItem {
  public line = input.required<LineDto>();
  public isSelected = input<boolean>(false);
  public selected = output<LineDto>();

  public selectLine() {
    this.selected.emit(this.line());
  }
}


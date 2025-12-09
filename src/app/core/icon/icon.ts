import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { IconType } from './icon-type';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-icon',
  imports: [NgClass],
  templateUrl: './icon.html',
  styleUrl: './icon.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Icon {
  public icon = input<IconType>('none');
}

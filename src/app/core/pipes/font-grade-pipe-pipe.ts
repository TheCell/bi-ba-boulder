import { Pipe, type PipeTransform } from '@angular/core';
import { FontGrade } from '../enums/font-grade.enum';

@Pipe({
  name: 'appFontGradePipe',
})
export class FontGradePipePipe implements PipeTransform {

  transform(value: undefined | number): string {
    if (!value) {
      return '';
    }

    return Object.values(FontGrade)[value];
  }

}

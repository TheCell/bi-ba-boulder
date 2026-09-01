import { Component, model } from '@angular/core';

@Component({
  selector: 'app-image-url-list',
  templateUrl: './image-url-list.html',
  styleUrl: './image-url-list.scss'
})
export class ImageUrlList {
  public images = model.required<string[]>();

  public onImageChanged(index: number, event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.images.update((items) => items.map((item, i) => (i === index ? value : item)));
  }

  public addImage(): void {
    this.images.update((items) => [...items, '']);
  }

  public removeImage(index: number): void {
    this.images.update((items) => items.filter((_, i) => i !== index));
  }
}

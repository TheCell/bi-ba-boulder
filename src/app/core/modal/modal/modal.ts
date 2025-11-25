import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Inject, inject, OnDestroy, OnInit } from '@angular/core';
import { ModalService } from '../modal.service';
import { NgClass } from '@angular/common';
// import { Subscription } from 'rxjs';

@Component({
  selector: 'app-modal',
  imports: [NgClass],
  templateUrl: './modal.html',
  styleUrl: './modal.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class Modal implements OnInit, OnDestroy {
  public id: string = 'modal'.appendUniqueId();
  public isOpen = false;

  private element: HTMLElement;

  public constructor(
    private modalService: ModalService,
    private elementRef: ElementRef,
    private changeDetectorRef: ChangeDetectorRef) {
      this.element = this.elementRef.nativeElement;
  }

  public ngOnDestroy(): void {
    this.modalService.remove(this.id);
    this.element.remove();
  }

  public ngOnInit(): void {
    this.modalService.add(this);
    document.body.appendChild(this.element);
  }

  public open(): void {
    this.isOpen = true;
    console.log('open');
    this.changeDetectorRef.markForCheck();
    
  }

  public close(): void {
    this.isOpen = false;
    console.log('close');
    this.changeDetectorRef.markForCheck();
  }

}

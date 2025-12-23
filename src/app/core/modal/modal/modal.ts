import { ChangeDetectionStrategy, Component, ComponentRef, ElementRef, inject, input, OnDestroy, OnInit, output, Type, ViewChild, ViewContainerRef } from '@angular/core';
import { ModalService } from '../modal.service';
import { NgClass } from '@angular/common';
import { IModal } from './modal.interface';
import { CloseModalEvent } from './close-modal-event';

@Component({
  selector: 'app-modal',
  imports: [NgClass],
  templateUrl: './modal.html',
  styleUrl: './modal.scss',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class Modal implements OnInit, OnDestroy {
  @ViewChild('dynamicContent', { read: ViewContainerRef }) public dynamicContent!: ViewContainerRef;
  @ViewChild('noButton') public noButton!: ElementRef<HTMLElement>;
  
  private modalService = inject(ModalService);
  private elementRef = inject(ElementRef);
  public isSmall = input<boolean>(false);
  public closed = output<CloseModalEvent>();

  public id: string = 'modal'.appendUniqueId();
  public isOpen = false;
  public showAskForPermissionToClose = false;

  private element: HTMLElement;
  private componentRef?: ComponentRef<IModal>;

  public constructor() {
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

  public onPermissionToCloseGranted(): void {
    this.closeModal();
  }

  public onPermissionToCloseDenied(): void {
    this.showAskForPermissionToClose = false;
  }

  public openWithExternalContent(): void {
    this.isOpen = true;
  }

  public open<T extends IModal>(component: Type<T>): IModal {
    this.dynamicContent.clear();
    this.componentRef = this.dynamicContent.createComponent(component);
    this.isOpen = true;
    return this.componentRef.instance;
  }

  public close(closeEvent: CloseModalEvent): void {
    if (this.componentRef?.instance.canCloseWithoutPermission === false) {
      this.showAskForPermissionToClose = true;
      setTimeout(() => {
        this.noButton.nativeElement.focus();
      });
      return;
    }

    this.closeModal();
    this.closed.emit(closeEvent);
  }

  private closeModal(): void {
    this.showAskForPermissionToClose = false;
    this.resetComponent();
    this.isOpen = false;
  }

  private resetComponent(): void {
    this.dynamicContent.clear();
    this.componentRef = undefined;
  }

}

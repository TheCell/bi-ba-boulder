import { ChangeDetectionStrategy, Component, ComponentRef, ElementRef, input, OnDestroy, OnInit, Type, ViewChild, ViewContainerRef } from '@angular/core';
import { ModalService } from '../modal.service';
import { NgClass } from '@angular/common';

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
  public isSmall = input<boolean>(false);

  public id: string = 'modal'.appendUniqueId();
  public isOpen = false;
  public showAskForPermissionToClose = false;

  private element: HTMLElement;
  private componentRef?: ComponentRef<any>;

  public constructor(
    private modalService: ModalService,
    private elementRef: ElementRef) {
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

  public open<T>(component: Type<T>): void {
    this.dynamicContent.clear();
    this.componentRef = this.dynamicContent.createComponent(component);
    this.isOpen = true;
  }

  public close(): void {
    if (this.componentRef?.instance.canCloseWithoutPermission === false) {
      this.showAskForPermissionToClose = true;
      setTimeout(() => {
        this.noButton.nativeElement.focus();
      });
      return;
    }
    this.closeModal();
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

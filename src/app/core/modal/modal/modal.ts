import {
  ChangeDetectorRef,
  Component,
  ComponentRef,
  ElementRef,
  inject,
  input,
  OnDestroy,
  OnInit,
  output,
  signal,
  Type,
  ViewChild,
  ViewContainerRef
} from '@angular/core';
import { ModalService } from '../modal.service';
import { NgClass } from '@angular/common';
import { IModal } from './modal.interface';
import { CloseModalEvent } from './close-modal-event';

@Component({
  selector: 'app-modal',
  imports: [NgClass],
  templateUrl: './modal.html',
  styleUrl: './modal.scss'
})
export class Modal implements OnInit, OnDestroy {
  @ViewChild('dynamicContent', { read: ViewContainerRef }) public dynamicContent!: ViewContainerRef;
  @ViewChild('noButton') public noButton?: ElementRef<HTMLElement>;

  private modalService = inject(ModalService);
  private elementRef = inject(ElementRef);
  private changeDetectorRef = inject(ChangeDetectorRef);

  public isSmall = input<boolean>(false);
  public closed = output<CloseModalEvent>();

  public id: string = 'modal'.appendUniqueId();
  public isOpen = signal(false);
  public showAskForPermissionToClose = signal(false);

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
    this.showAskForPermissionToClose.set(false);
  }

  public openWithExternalContent(): void {
    this.isOpen.set(true);
  }

  public open<T extends IModal>(component: Type<T>): IModal {
    this.dynamicContent.clear();
    this.componentRef = this.dynamicContent.createComponent(component);
    this.isOpen.set(true);
    this.changeDetectorRef.markForCheck();
    this.componentRef.instance.closeModal.subscribe((closeModalEvent: CloseModalEvent) => {
      this.close(closeModalEvent);
    });
    return this.componentRef.instance;
  }

  public close(closeEvent: CloseModalEvent): void {
    if (this.componentRef?.instance.canCloseWithoutPermission === false) {
      this.showAskForPermissionToClose.set(true);
      setTimeout(() => {
        this.noButton?.nativeElement.focus();
      });
      return;
    }

    this.closeModal();
    this.closed.emit(closeEvent);
  }

  private closeModal(): void {
    this.showAskForPermissionToClose.set(false);
    this.resetComponent();
    this.isOpen.set(false);
    this.changeDetectorRef.markForCheck();
    this.closed.emit({ closeType: 1 });
  }

  private resetComponent(): void {
    this.dynamicContent.clear();
    this.componentRef = undefined;
  }
}

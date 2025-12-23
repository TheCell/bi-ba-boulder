import { OutputEmitterRef } from "@angular/core";
import { CloseModalEvent } from "./close-modal-event";

export interface IModal {
  canCloseWithoutPermission: boolean;
  initialize?(data: object): void;
  onClose?(): void;
  closeModal: OutputEmitterRef<CloseModalEvent>;
}
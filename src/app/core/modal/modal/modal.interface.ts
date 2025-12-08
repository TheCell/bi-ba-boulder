export interface iModal {
  canCloseWithoutPermission: boolean;
  initialize?(data: object): void;
  onClose?(): void;
}
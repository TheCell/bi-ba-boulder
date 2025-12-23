export interface IModal {
  canCloseWithoutPermission: boolean;
  initialize?(data: object): void;
  onClose?(): void;
}
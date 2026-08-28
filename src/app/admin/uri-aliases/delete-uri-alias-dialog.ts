import { Component, output } from '@angular/core';
import { UriAliasAdministrationDto } from '@api-net/index';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { UriAliasDialogData } from './uri-alias-dialog-data';

@Component({
  selector: 'app-delete-uri-alias-dialog',
  templateUrl: './delete-uri-alias-dialog.html'
})
export class DeleteUriAliasDialog implements IModal {
  public closeModal = output<CloseModalEvent>();
  public canCloseWithoutPermission = true;
  public uriAlias: UriAliasAdministrationDto = null!;

  public initialize(data: UriAliasDialogData): void {
    this.uriAlias = data.uriAlias!;
  }
}

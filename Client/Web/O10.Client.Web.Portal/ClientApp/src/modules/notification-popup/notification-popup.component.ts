import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface DialogData {
  title: string;
  messages: string[];
}

@Component({
  selector: 'notification-popup',
  templateUrl: 'notification-popup.component.html',
})
export class NotificationPopupDialog {
  constructor(@Inject(MAT_DIALOG_DATA) public data: DialogData) { }

}

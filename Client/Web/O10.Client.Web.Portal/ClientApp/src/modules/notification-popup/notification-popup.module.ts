import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { NotificationPopupDialog } from './notification-popup.component';

@NgModule({
  declarations: [NotificationPopupDialog],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    MatDialogModule, MatButtonModule
  ],
  bootstrap: [NotificationPopupDialog]
})
export class NotificationPopupModule { }

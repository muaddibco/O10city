import { NgModule } from '@angular/core';
import { QrCodeExModule } from '../qrcode/qrcode.module';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { QrCodePopupComponent } from './qrcode-popup.component';

@NgModule({
  declarations: [QrCodePopupComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    MatBottomSheetModule,
    QrCodeExModule
  ],
  bootstrap: [QrCodePopupComponent]
})
export class QrCodePopupModule { }

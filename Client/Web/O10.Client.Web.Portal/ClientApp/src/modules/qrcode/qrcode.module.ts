import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { QrCodeComponent } from './qrcode.component';
import { QRCodeModule } from 'angularx-qrcode';

@NgModule({
  declarations: [QrCodeComponent],
  imports: [BrowserModule, QRCodeModule],
  exports: [QrCodeComponent],
  bootstrap: [QrCodeComponent]
})
export class QrCodeExModule { }

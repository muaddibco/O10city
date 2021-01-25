import { Component, Inject, Input } from '@angular/core';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';

export interface PopupData {
  qrCode: string;
}

@Component({
  templateUrl: './qrcode-popup.component.html'
})
export class QrCodePopupComponent {
  constructor(
    public dialogRef: MatBottomSheetRef<QrCodePopupComponent>,
    @Inject(MAT_BOTTOM_SHEET_DATA) public data: PopupData
  ) { }

  onCopiedEvent() {
    this.dialogRef.dismiss();
  }
}

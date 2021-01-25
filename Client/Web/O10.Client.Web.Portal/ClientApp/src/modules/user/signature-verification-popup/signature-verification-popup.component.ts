import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DocumentSignatureVerification } from '../user.Service'

@Component({
  templateUrl: 'signature-verification-popup.component.html',
})
export class SignatureVerificationPopup {
  constructor(public dialogRef: MatDialogRef<SignatureVerificationPopup>,
    @Inject(MAT_DIALOG_DATA) public data: DocumentSignatureVerification) {

  }
}

import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { RelationProofsValidationResults } from '../user.Service';

@Component({
  templateUrl: 'relations-validation-popup.component.html',
})
export class RelationsValidationPopup {
  constructor(public dialogRef: MatDialogRef<RelationsValidationPopup>,
    @Inject(MAT_DIALOG_DATA) public data: RelationProofsValidationResults) {

  }

  onCancelClick(): void {
    this.dialogRef.close();
  }
}

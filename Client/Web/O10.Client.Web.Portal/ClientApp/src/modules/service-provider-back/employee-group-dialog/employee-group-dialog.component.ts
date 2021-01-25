import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface EmployeeGroupDialogData {
  groupId: number;
  groupName: string;
  isAdding: boolean;
}

@Component({
  templateUrl: 'employee-group-dialog.component.html',
})
export class DialogEmployeeGroupDialog {

  constructor(
    public dialogRef: MatDialogRef<DialogEmployeeGroupDialog>,
    @Inject(MAT_DIALOG_DATA) public data: EmployeeGroupDialogData) { }


  onCancelClick(): void {
    this.dialogRef.close();
  }

}

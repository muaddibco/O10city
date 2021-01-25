import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { EmployeeGroup, EmployeeRecord } from '../serviceProvider.service';

export interface EmployeeRecordDialogData {
  employeeId: number;
  description: string;
  rawRootAttribute: string;
  registrationCommitment: string;
  groupId: number;
  groups: EmployeeGroup[];
  isAdding: boolean;
  model: EmployeeRecord;
}

@Component({
  templateUrl: 'employee-record-dialog.component.html',
})
export class DialogEmployeeRecordDialog {

  constructor(
    public dialogRef: MatDialogRef<DialogEmployeeRecordDialog>,
    @Inject(MAT_DIALOG_DATA) public data: EmployeeRecordDialogData) { }


  onCancelClick(): void {
    this.dialogRef.close();
  }

}

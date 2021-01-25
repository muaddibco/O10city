import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

export interface AddTransactionDialogData {
	registration: string;
	description: string;
}


@Component({
	selector: 'dialog-add-transaction',
	templateUrl: 'add-transaction.dialog.html',
	styleUrls: ['add-transaction.dialog.scss']
})
export class AddTransactionDialog {
	constructor(
		public dialogRef: MatDialogRef<AddTransactionDialog>,
		@Inject(MAT_DIALOG_DATA) public data: AddTransactionDialogData) { }

	onCancelClick(): void {
		this.dialogRef.close();
	}
}

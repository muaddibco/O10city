import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormControl, Validators } from '@angular/forms';

export interface ConsentConfirmData {
	description: string;
}

@Component({
	templateUrl: 'consent-confirm.dialog.html',
})
export class ConsentConfirmDialog {
	passwordInput = new FormControl('', [Validators.required]);
	password: string = '';

	constructor(
		public dialogRef: MatDialogRef<ConsentConfirmDialog>,
		@Inject(MAT_DIALOG_DATA) public data: ConsentConfirmData) { }

	onCancelClick(): void {
		this.dialogRef.close();
	}

}

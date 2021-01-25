import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormControl, Validators } from '@angular/forms';

export interface PasswordConfirmData {
	title: string;
	confirmButtonText: string;
	cancelButtonText: string;
}

@Component({
	templateUrl: 'password-confirm.dialog.html',
})
export class PasswordConfirmDialog {

	passwordInput = new FormControl('', [Validators.required]);
	password: string = '';

	confirmButtonText: string;
	cancelButtonText: string;
	title: string;


	constructor(
		public dialogRef: MatDialogRef<PasswordConfirmDialog>,
		@Inject(MAT_DIALOG_DATA) public data: PasswordConfirmData) {
		if (data.title) {
			this.title = data.title;
		} else {
			this.title = "Confirm secrets disclosure";
		}
		if (data.confirmButtonText) {
			this.confirmButtonText = data.confirmButtonText;
		}
		else {
			this.confirmButtonText = "OK";
		}
		if (data.cancelButtonText) {
			this.cancelButtonText = data.cancelButtonText;
		}
		else {
			this.cancelButtonText = "Cancel";
		}
	}

	onCancelClick(): void {
		this.dialogRef.close();
	}
} 

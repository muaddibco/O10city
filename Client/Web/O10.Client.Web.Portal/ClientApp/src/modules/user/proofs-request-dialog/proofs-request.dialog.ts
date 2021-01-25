import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ProofsRequest } from '../user.Service';

@Component({
	templateUrl: 'proofs-request.dialog.html',
})
export class ProofsRequestDialog {
	constructor(
		private dialogRef: MatDialogRef<ProofsRequestDialog>,
		@Inject(MAT_DIALOG_DATA) public data: ProofsRequest) { }

	onCancelClick(): void {
		this.dialogRef.close();
	}

	onToggleKnowledgeFactor(evt) {
		this.data.withKnowledgeProof = evt.checked;
	}

	onToggleBiometricFactor(evt) {
		this.data.withBiometricProof = evt.checked;
	}
}

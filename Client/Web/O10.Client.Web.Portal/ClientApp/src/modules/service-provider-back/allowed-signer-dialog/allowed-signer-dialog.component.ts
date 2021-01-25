import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ServiceProviderRelationGroups, RelationGroup } from '../serviceProvider.service';

export interface DialogData {
    documentId: number;
    spRelationGroups: ServiceProviderRelationGroups[];
    selectedServiceProvider: ServiceProviderRelationGroups;
    selectedRelationGroup: RelationGroup;
}

@Component({
    selector: 'allowed-signer',
    templateUrl: 'allowed-signer-dialog.component.html',
})
export class AllowedSignerDialog {
    constructor(
        public dialogRef: MatDialogRef<AllowedSignerDialog>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData) { }

    onCancelClick(): void {
        this.dialogRef.close();
    }
}

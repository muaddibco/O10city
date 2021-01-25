import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AttributeDefinition } from '../identities.service';

export interface AttributeDefinitionWithValue extends AttributeDefinition {
    valueType: string;
    value: string;
}

export interface DialogData {
    description: string;
    attributeDefinitions: AttributeDefinitionWithValue[];
}

@Component({
    templateUrl: 'add-identity.dialog.html',
})
export class AddIdentityDialog {
    constructor(public dialogRef: MatDialogRef<AddIdentityDialog>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData) { }

    onCancelClick(): void {
        this.dialogRef.close();
    }
}

import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSelectChange } from '@angular/material/select';
import { SchemeItem, AttributeDefinition } from '../identities.service';
import { FormControl, Validators } from '@angular/forms';

export interface DialogData {
    schemeItems: SchemeItem[];
    attributeDefinition: AttributeDefinition;
}

@Component({
    templateUrl: 'add-attribute-dialog.component.html',
})
export class AddAttributeDialog {

    attributeName = new FormControl('', [Validators.required]);
    attributeAlias = new FormControl('', [Validators.required]);

    constructor(
        public dialogRef: MatDialogRef<AddAttributeDialog>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData) { }

    onCancelClick(): void {
        this.dialogRef.close();
    }

    onSelectionChange(evt: MatSelectChange) {
        //console.log("onSelectionChange: " + JSON.stringify(evt));
        const item: SchemeItem = evt.value;
        this.data.attributeDefinition.schemeName = item.name;
    }
}

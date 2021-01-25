import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSelectChange } from '@angular/material/select';
import { IdentityAttributeValidationDescriptorDto } from '../serviceProvider.service';

export interface DialogData {
    identityAttributeValidationDescriptors: IdentityAttributeValidationDescriptorDto[];
    schemeAliases: string[];
    selectedSchemeName: string;
    ageRestriction: string;
}

@Component({
    selector: 'dialog-add-validation',
    templateUrl: 'add-validation-dialog.component.html',
})
export class DialogAddValidationDialog {
    public showAgeCriterionValue: boolean;

    constructor(
        public dialogRef: MatDialogRef<DialogAddValidationDialog>,
        @Inject(MAT_DIALOG_DATA) public data: DialogData) {
        this.showAgeCriterionValue = false;
    }

    onCancelClick(): void {
        this.dialogRef.close();
    }

    onSelectionChange(evt: MatSelectChange) {
        //console.log("onSelectionChange: " + JSON.stringify(evt));
        this.data.selectedSchemeName = this.getSchemeName(evt.value);
        if (this.data.selectedSchemeName === "DateOfBirth") {
            this.showAgeCriterionValue = true;
        }
        else {
            this.showAgeCriterionValue = false;
        }
    }

    getSchemeName(schemeAlias: string) {
        for (let validationDesc of this.data.identityAttributeValidationDescriptors) {
            if (validationDesc.schemeAlias == schemeAlias) {
                return validationDesc.schemeName;
            }
        }

        return "";
    }
}

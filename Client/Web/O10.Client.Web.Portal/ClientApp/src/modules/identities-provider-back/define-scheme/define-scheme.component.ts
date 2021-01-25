import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
//import { MatAlertDialog, MatConfirmDialog, MatConfirmDialogData, MatAlertDialogData } from '@angular-material-extensions/core';
import { IdentitiesService, SchemeItem, AttributeDefinition } from '../identities.service'
import { Title } from '@angular/platform-browser';

import { AddAttributeDialog } from '../add-attribute-dialog/add-attribute-dialog.component'
import { Router } from '@angular/router';

@Component({
    templateUrl: './define-scheme.component.html'
})
export class DefineSchemeComponent implements OnInit {

    private schemeItems: SchemeItem[];
    private attributeDefinitions: AttributeDefinition[];
    private issuer: string;
    private oldRootAttribute: AttributeDefinition;
    public isLoaded = false;
    public areNewAttributes = false;
    public isRootChanged = false;

    constructor(private service: IdentitiesService, titleService: Title, private dialog: MatDialog, private router: Router) {
        let tokenInfo = JSON.parse(sessionStorage.getItem('TokenInfo'));
        titleService.setTitle(tokenInfo.accountInfo + " Scheme Definition");
        this.issuer = tokenInfo.publicSpendKey;
    }

    ngOnInit() {
        this.service.getSchemeItems().subscribe(
            r => {
                this.schemeItems = r;
            },
            e => {
                console.error(e);
                alert("Error while obtaining Scheme Items");
            }
        );

        this.service.getAttributeDefinitions(this.issuer).subscribe(
            r => {
                this.attributeDefinitions = r;
                this.oldRootAttribute = r.find(v => v.isRoot);
                this.isLoaded = true;
            },
            e => {
                console.error(e);
                alert("Error while obtaining Attribute Definitions");
            }
        );
    }

    openAddAttributeDialog() {
        const attributeDefinition: AttributeDefinition = {
            schemeId: 0,
            schemeName: "",
            attributeName: "",
            alias: "",
            description: "",
            isActive: true,
            isRoot: false
        };
        const dialogRef = this.dialog.open(AddAttributeDialog, {
            width: '600px',
            data: {
                schemeItems: this.schemeItems, attributeDefinition: attributeDefinition}
        });

        dialogRef.afterClosed().subscribe(result => {
            let isUnique: boolean = true;

            for (let attributeDefinition of this.attributeDefinitions) {
                if (attributeDefinition.schemeName === result.attributeDefinition.schemeName) {
                    isUnique = false;
                    break;
                }
            }

            if (isUnique) {
                let attributeDefinition: AttributeDefinition = result.attributeDefinition;

                this.attributeDefinitions.push(attributeDefinition);
                this.setNewAttributesFlag();
            }
			else {
				//const alertData: MatAlertDialogData = {
				//	icon: " ",
				//	message: "Attribute with Scheme Name" + result.attributeDefinition.schemeName + " already exist",
				//	title: "Attribute Definition Error",
				//	okTextButton: "OK",
				//	type: "primary"
    //          };
				//this.dialog.open(MatAlertDialog, { width:"600px", data: alertData });
          alert("Attribute with Scheme Name" + result.attributeDefinition.schemeName + " already exist");
          }
        });
    }

    setNewAttributesFlag() {
        for (const attributeDefinition of this.attributeDefinitions) {
            if (attributeDefinition.schemeId === 0) {
                this.areNewAttributes = true;
                return;
            }
        }

        this.areNewAttributes = false;
    }

    detectRootChanged() {
        const rootAttr = this.attributeDefinitions.find(v => v.isRoot);

        this.isRootChanged = this.oldRootAttribute != rootAttr;
    }

    onSaveScheme() {
        this.service.saveScheme(this.issuer, this.attributeDefinitions).subscribe(
            r => {
                this.attributeDefinitions = r;
                this.router.navigate(['/identityProvider']);
            },
            e => {
                console.error(e);
                alert("Error while saving Attribute Definitions Scheme");
            }
        );
    }

    onCancel() {
        this.router.navigate(['/identityProvider']);
    }

    onIsRootChanged(evt) {
        const isChecked: boolean = evt.checked;

        const schemeName: string = evt.source.id;

        for (let attributeDefinition of this.attributeDefinitions) {
            if (attributeDefinition.schemeName != schemeName) {
                attributeDefinition.isRoot = false;
            }
            else {
                attributeDefinition.isRoot = isChecked;
            }
        }

        this.detectRootChanged();
    }

    dismissAttributeDefinition(attributeDefinition: AttributeDefinition) {
        //const data: MatConfirmDialogData = {
        //    title: "Attribute Definition Removing",
        //    confirmMessage: "Are you sure you want to delete Attribute Definition with Scheme Name " + attributeDefinition.schemeName + "?",
        //    confirmTextButton: "Yes",
        //    cancelTextButton: "Cancel"
        //};

        //this.dialog.open(MatConfirmDialog, { data: data }).afterClosed()
        //    .subscribe(r => {
        //        if (r) {
        //            const index = this.attributeDefinitions.findIndex(a => a.schemeName === attributeDefinition.schemeName);
        //            this.attributeDefinitions.splice(index);
        //            this.setNewAttributesFlag();
        //        }
        //    });
      if (confirm("Are you sure you want to delete Attribute Definition with Scheme Name " + attributeDefinition.schemeName + "?")) {
        const index = this.attributeDefinitions.findIndex(a => a.schemeName === attributeDefinition.schemeName);
        this.attributeDefinitions.splice(index);
        this.setNewAttributesFlag();
      }
    }
}

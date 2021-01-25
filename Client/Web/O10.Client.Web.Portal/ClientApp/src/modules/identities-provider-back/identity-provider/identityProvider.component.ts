import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { first } from 'rxjs/operators';
import { Title } from '@angular/platform-browser';
import { IdentityDto, IdentityAttributeDto, IdentityAttributesSchemaDto, SchemeItem, AttributeDefinition, IdentitiesService } from '../identities.service';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import { MatDialog } from '@angular/material/dialog';
import { AddIdentityDialog, AttributeDefinitionWithValue, DialogData } from '../add-identity-dialog/add-identity.dialog'
import { Router } from '@angular/router';
import { QrCodePopupComponent } from '../../qrcode-popup/qrcode-popup.component';

@Component({
    templateUrl: './identityProvider.component.html'
})

export class IdentityProviderComponent implements OnInit {

    public pageTitle: string;
    public isLoaded: boolean;
    public accountId: number;
    public identities: IdentityDto[];
    private schemeItems: SchemeItem[];
    private attributeDefinitions: AttributeDefinition[];
    public qrCode: string;
    public identityAttributesSchema: IdentityAttributesSchemaDto;
    error = '';
    public imageContent: string | ArrayBuffer;
  private issuer: string;


    constructor(
        private http: HttpClient,
        @Inject('BASE_URL') private baseUrl: string,
        private identityService: IdentitiesService,
		    private dialog: MatDialog,
		    private bottomSheet: MatBottomSheet,
        private router: Router,
        titleService: Title) {
        this.isLoaded = false;
        let tokenInfo = JSON.parse(sessionStorage.getItem('TokenInfo'));
        this.accountId = tokenInfo.accountId;
        this.pageTitle = tokenInfo.accountInfo;
        this.issuer = tokenInfo.publicSpendKey;
        this.qrCode = btoa("iss://" + baseUrl + "IdentityProvider/IssuanceDetails?issuer=" + this.issuer);
        titleService.setTitle(tokenInfo.accountInfo + " Back Office");
    }

    ngOnInit() {

        this.getAllIdentities();

        this.initializeScheme();
    }

    private getAllIdentities() {
        this.http.get<IdentityDto[]>(this.baseUrl + 'IdentityProvider/GetAllIdentities/' + this.accountId).subscribe(r => {
            this.identities = r;
            this.isLoaded = true;
        });
    }

    private initializeScheme() {
        this.identityService.getSchemeItems().subscribe(
            r => {
                this.schemeItems = r;
            },
            e => {
                console.error(e);
                alert("Error while obtaining Scheme Items");
            }
        );

        this.identityService.getAttributeDefinitions(this.issuer).subscribe(
            r => {
                this.attributeDefinitions = r;
            },
            e => {
                console.error(e);
                alert("Error while obtaining Attribute Definitions");
            }
        );

    }

    addIdentity() {
        let attrs: AttributeDefinitionWithValue[] = [];

        for (const attributeDefinition of this.attributeDefinitions) {
            let attr: AttributeDefinitionWithValue = {
                ...attributeDefinition,
                valueType: this.getValueType(attributeDefinition.schemeName),
                value: ""
            }

            attrs.push(attr);
        }

        let data: DialogData = {
            description:"",
            attributeDefinitions: attrs
        }

        const dialogRef = this.dialog.open(AddIdentityDialog, { width: "600px", data: data });

        dialogRef.afterClosed().subscribe(r => {
            if (r) {

                var identity = this.createIdentity(r.description, r.attributeDefinitions);

                this.addIdentityDto(identity);
            }
        });
    }

    getValueType(schemeName: string) {
        for (const item of this.schemeItems) {
            if (item.name === schemeName) {
                return item.valueType;
            }
        }

        return undefined;
	}

	getRootAttributeDefinition() {
		for (const item of this.attributeDefinitions) {
			if (item.isRoot) {
				return item;
			}
		}
	}

	getRootAttributeValue(identity: IdentityDto) {
		let rootAttributeDefinition: AttributeDefinition = this.getRootAttributeDefinition();
		if (rootAttributeDefinition) {
			for (const item of identity.attributes) {
				if (item.attributeName === rootAttributeDefinition.attributeName) {
					return item.content;
				}
			}
		}
	}

    private createIdentity(description: string, attributes: AttributeDefinitionWithValue[]) {
        let identityAttributes: IdentityAttributeDto[] = [];

        for (let attr of attributes) {
            let identityAttribute: IdentityAttributeDto = {
                attributeName: attr.attributeName,
                content: attr.value,
                originatingCommitment: ""
            };

            identityAttributes.push(identityAttribute);
        }

        var identity: IdentityDto =
        {
            description: description,
            numberOfTransfers: 0,
            id: "",
            attributes: identityAttributes
        };

        return identity;
    }

    private addIdentityDto(identity: IdentityDto) {
        this.identityService.addIdentity(this.accountId, identity)
            .pipe(first())
            .subscribe(data => {
                identity.id = data;
                this.identities.push(identity);
                this.error = '';
            }, error => {
                this.error = 'Failed to create identity';
            });
    }

    onFileChange(event) {
        const reader = new FileReader();

        if (event.target.files && event.target.files.length) {
            const [file] = event.target.files;
            reader.readAsDataURL(file);

            reader.onload = () => {
                this.imageContent = reader.result;
                console.log(this.imageContent);
            };
        }
    }

    defineScheme() {
        this.router.navigate(["/defineScheme"]);
    }

    viewIdentity(id: string) {
        this.router.navigate(["/view-identity", id]);
    }

  	onShowQrClick() {
			this.bottomSheet.open(QrCodePopupComponent, { data: { qrCode: this.qrCode } });
	  }
}

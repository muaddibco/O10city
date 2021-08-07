import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ActionType, UserService, UserAttributeDto, UserAttributeLastUpdateDto, DiscloseSecretsQR, UserAssociatedAttributeDto, UnauthorizedUseDto, DocumentSignatureVerification, GroupRelation, RelationProofsValidationResults, SchemeItem, ProofsChallenge, ConsentRequest, UserRegistration } from './user.Service';
import { first } from 'rxjs/operators';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { Title } from '@angular/platform-browser';
import { DomSanitizer } from '@angular/platform-browser';
import { SignatureVerificationPopup } from './signature-verification-popup/signature-verification-popup.component'
import { MatBottomSheet, MatBottomSheetRef } from '@angular/material/bottom-sheet';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { QrCodePopupComponent } from '../qrcode-popup/qrcode-popup.component';
import { RelationsValidationPopup } from './relations-validation-popup/relations-validation-popup.component';
import { PasswordConfirmDialog } from './password-confirm-dialog/password-confirm.dialog';
import { ConsentConfirmDialog } from './consent-confirm-dialog/consent-confirm.dialog';
import { ProofsRequestDialog } from './proofs-request-dialog/proofs-request.dialog';
import { CookieService } from 'ngx-cookie-service';
//import { MatAlertDialog, MatAlertDialogData } from '@angular-material-extensions/core';
import { Guid } from 'guid-typescript';

class UserAttribute {
    constructor(issuer: string, issuerName: string, assetId: string, content: string) {
        this.issuer = issuer;
        this.issuerName = issuerName;
        this.assetId = assetId;
        this.content = content;
        this.attrPhoto = null;
        this.rootAttributes = [];
        this.userAssociatedAttributes = [];
        this.groupRelations = [];
        this.status = 0;
    }

    public status: number;
    public issuer: string;
    public issuerName: string;
    public assetId: string;
    public content: string;
    public attrPhoto: UserAssociatedAttributeDto;
    public rootAttributes: UserAttributeDto[];
    public userAssociatedAttributes: UserAssociatedAttributeDto[];
    public groupRelations: GroupRelation[];
}

@Component({
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent implements OnInit {

  private accountId: number;
  public action: string;
  public actionError: string;
  public isActionError: boolean;
  public actionText: string = "";
  public target: string;
  public payload: string;

  public hubConnection: HubConnection;
  public consentHub: HubConnection;
  public spendKey: string;
  public viewKey: string;
  public pageTitle: string;
  public isLoaded: boolean;
  public isCompromised: boolean;
  public isAutoTheftProtection: boolean = true;

  error = '';
  public qrContent: string;
  public isMobile = false;
  public activateQrScanner = false;
  public device: MediaDeviceInfo;
  public devices: MediaDeviceInfo[];
  private selectedDevice: number;

  public schemeItems: SchemeItem[];
  public userAttributes: UserAttribute[] = [];
  public unauthorizedUse: UnauthorizedUseDto;

  private validationResultsDialogRef: MatDialogRef<RelationsValidationPopup>;
  private qrCodeSheetRef: MatBottomSheetRef<QrCodePopupComponent> = null;

  constructor(
    private userService: UserService,
    private router: Router,
    titleService: Title,
    private sanitizer: DomSanitizer,
    private cookieService: CookieService,
    public dialog: MatDialog,
    private bottomSheet: MatBottomSheet) {
    this.isLoaded = false;
    this.unauthorizedUse = null;
    let tokenInfo = JSON.parse(sessionStorage.getItem('TokenInfo'));
    this.accountId = tokenInfo.accountId;
    titleService.setTitle(tokenInfo.accountInfo);
    this.userAttributes = [];
  }


  ngOnInit() {
    var ua = navigator.userAgent;
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua)) {
      this.isMobile = true;
    } else {
      this.isMobile = false;
    }

    this.initializeHub();

    this.getUserDetails();

    this.initializeUserAttributes();

  }

	private initializeConsentHub(consentHubUri: string) {
		this.consentHub = new HubConnectionBuilder().withUrl(consentHubUri).build();

		this.consentHub.on("ChallengeProofs", (i) => {
			console.log("ChallengeProofs");
			console.log(i);
			const proofsChallenge: ProofsChallenge = i as ProofsChallenge;
			if (proofsChallenge) {
				if (this.qrCodeSheetRef) {
					this.qrCodeSheetRef.dismiss();
				}
				this.ProvideProofs(proofsChallenge.publicSpendKey, proofsChallenge.publicViewKey, proofsChallenge.sessionKey, proofsChallenge.withKnowledgeProof, proofsChallenge.withBiometricProof);
			}
		});

		this.consentHub.on("ValidationResults", (i) => {
			const validationResults = i as RelationProofsValidationResults;
			console.log(validationResults);
			this.validationResultsDialogRef = this.dialog.open(RelationsValidationPopup, { data: validationResults });
			this.validationResultsDialogRef.afterClosed().subscribe(r => { this.validationResultsDialogRef = null; });
		});

		this.consentHub.on("ProofsCompromised", (i) => {
			console.log("ProofsCompromised");
			console.log(i);
			if (this.validationResultsDialogRef) {
				this.validationResultsDialogRef.componentInstance.data.isCompromised = true;
			}
		});

		this.consentHub.on("ConsentRequest", o => {
			const consentRequest: ConsentRequest = o as ConsentRequest;
			if (consentRequest) {
				const json = JSON.stringify(consentRequest);
				const actionInfo = 'cnsn://' + json;
				const actionInfoEncoded = btoa(actionInfo);

				this.ServiceProvider(actionInfoEncoded);
				//const dialogRef = this.dialog.open(ConsentConfirmDialog, { data: { description: consentRequest.description } });
				//dialogRef.afterClosed().subscribe(r => {
				// if (r) {
				// }
				//});
			}
		});

		this.consentHub.onclose(e => {
			console.log("consentHub.onclose: [" + e.name + "] " + e.message);
			this.startConsentHubConnection();
		});

		this.startConsentHubConnection();
	}

  private initializeHub() {
    this.hubConnection = new HubConnectionBuilder().withUrl("/identitiesHub").build();

    this.hubConnection.on("PushAttribute", (i) => {
      this.attachRootAttribute(i as UserAttributeDto);
    });

    this.hubConnection.on("PushUserAttributeUpdate", (i) => {
      this.attachRootAttribute(i as UserAttributeDto);
    });

    this.hubConnection.on("PushUserAttributeLastUpdate", (i) => {
      var attributeUpdate = (i as UserAttributeLastUpdateDto);
      if (attributeUpdate != null) {
        var attributeToUpdate: UserAttributeDto;
        var userAttribute: UserAttribute;
        if (attributeUpdate.issuer) {
          userAttribute = this.userAttributes.find(t => t.issuer === attributeUpdate.issuer);
          if (userAttribute) {
            for (let rootAttr of userAttribute.rootAttributes) {
              if (rootAttr.assetId === attributeUpdate.assetId) {
                attributeToUpdate = rootAttr;
                break;
              }
            }
          }
        }
        else {
          for (let item of this.userAttributes) {
            for (let rootAttr of item.rootAttributes) {
              if (rootAttr.assetId === attributeUpdate.assetId) {
                userAttribute = item;
                attributeToUpdate = rootAttr;
                break;
              }
            }
          }
        }

        if (attributeToUpdate != null) {
          attributeToUpdate.lastBlindingFactor = attributeUpdate.lastBlindingFactor;
          attributeToUpdate.lastCommitment = attributeUpdate.lastCommitment;
          attributeToUpdate.lastDestinationKey = attributeUpdate.lastDestinationKey;
          attributeToUpdate.lastTransactionKey = attributeUpdate.lastTransactionKey;
        }
      }
    });

    this.hubConnection.on("PushUnauthorizedUse", (i) => {
      console.log("PushUnauthorizedUse");
      console.log(i);
      this.unauthorizedUse = i as UnauthorizedUseDto;
      console.log("that.isAutoTheftProtection = " + this.isAutoTheftProtection);

      if (this.isAutoTheftProtection && !this.isCompromised) {
		    console.log("sendCompromisedProofs");
  		  this.isCompromised = true;
        this.sendCompromisedProofs();
      }

    });

    this.hubConnection.on("PushGroupRelation", (i) => {
      const groupRelation = i as GroupRelation;
      let userAttribute = this.getUserAttributeByIssuer(groupRelation.issuer, groupRelation.assetId);
      if (userAttribute) {
        userAttribute.groupRelations.push(groupRelation);
      }
    });

	  this.hubConnection.on("PushUserRegistration", (i) => {
		  const reg = i as UserRegistration;
		  this.subscribeForConsentRequest(reg);
	  });

    this.hubConnection.onclose(e => {
      console.log("hubConnection.onclose: [" + e.name + "] " + e.message);
      this.startHubConnection();
    });

    this.startHubConnection();
  }

	private startConsentHubConnection() {
		console.log("starting Consent Hub connection...");
		const that = this;
		this.consentHub.start()
			.then(() => {
				console.log("Consent Hub started");
				that.getUserRegistrations();
			})
			.catch(err => {
				console.log("starting Consent Hub connection failed");
				console.error(err);
				setTimeout(() => this.startConsentHubConnection(), 1000);
			});
	}

  private startHubConnection() {
    console.log("starting hub connection...");
    this.hubConnection.start()
      .then(() => {
        const accountId = sessionStorage.getItem('AccountId');
        console.log("Hub started");
        this.hubConnection.invoke("AddToGroup", accountId);
      })
      .catch(err => {
        console.log("starting hub connection failed");
        console.error(err);
        setTimeout(() => this.startHubConnection(), 1000);
      });
  }

  private initializeUserAttributes() {
    this.userService.getSchemeItems().subscribe(
      r => {
        this.schemeItems = r;
      });
    this.userService.getUserAttributes(this.accountId).subscribe(r => {
      for (const item of r) {
        this.attachRootAttribute(item);
      }
      for (let userAttribute of this.userAttributes) {
        this.initializeUserAssociatedAttributes(userAttribute);
      }
      this.getGroupRelations();
    });

    setTimeout(() => this.updateUserAttributes(), 10000);
  }

  private attachRootAttribute(item: UserAttributeDto) {
    if (!item) {
      return;
    }
    console.log(item);
    let userAttribute = this.getUserAttributeByIssuer(item.source, item.assetId);
    if (!userAttribute) {
      userAttribute = new UserAttribute(item.source, item.issuerName, item.assetId, item.content);
      this.userAttributes.push(userAttribute);
    }

    for (let rootAttr of userAttribute.rootAttributes) {
      if (rootAttr.originalBlindingFactor == item.originalBlindingFactor) {
        rootAttr.isOverriden = item.isOverriden;
        userAttribute.status = this.getRootAggregatedStatus(userAttribute);
        return;
      }

      if (rootAttr.originalCommitment == "0000000000000000000000000000000000000000000000000000000000000000" && rootAttr.content == item.content) {
        rootAttr.assetId = item.assetId;
        rootAttr.schemeName = item.schemeName;
        rootAttr.lastBlindingFactor = item.lastBlindingFactor;
        rootAttr.lastCommitment = item.lastCommitment;
        rootAttr.lastDestinationKey = item.lastDestinationKey;
        rootAttr.lastTransactionKey = item.lastTransactionKey;
        rootAttr.originalBlindingFactor = item.originalBlindingFactor;
        rootAttr.originalCommitment = item.originalCommitment;
        rootAttr.source = item.source;
        rootAttr.validated = true;
        userAttribute.status = this.getRootAggregatedStatus(userAttribute);
        return;
      }
    }

    userAttribute.rootAttributes.push(item);
    userAttribute.status = this.getRootAggregatedStatus(userAttribute);
  }

  private findRootAttribute(id: number): UserAttributeDto {
    for (const item of this.userAttributes) {
      for (const attr of item.rootAttributes) {
        if (attr.userAttributeId === id) {
          return attr;
        }
      }
    }
  }

  private updateUserAttributes() {
    let needUpdate = this.userAttributes.length === 0;

    for (const item of this.userAttributes) {
      for (const attr of item.rootAttributes) {
        if (attr.lastCommitment === "0000000000000000000000000000000000000000000000000000000000000000") {
          needUpdate = true;
          break;
        }
      }
    }

    if (needUpdate) {
      console.log("Attributes will be updated");
      this.userService.getUserAttributes(this.accountId).subscribe(r => {
        for (var attr of r) {
          const userAttr = this.findRootAttribute(attr.userAttributeId);

          if (userAttr) {
            this.mergeUserAttributes(userAttr, attr);
          } else {
            this.attachRootAttribute(attr);
          }
        }
      });
    } else {
      console.log("No need for attributes update");
    }

    setTimeout(() => this.updateUserAttributes(), 10000);
  }

  private mergeUserAttributes(target: UserAttributeDto, source: UserAttributeDto) {
    target.assetId = source.assetId;
    target.schemeName = source.schemeName;
    target.content = source.content;
    target.isOverriden = source.isOverriden;
    target.lastBlindingFactor = source.lastBlindingFactor;
    target.lastCommitment = source.lastCommitment;
    target.lastDestinationKey = source.lastDestinationKey;
    target.lastTransactionKey = source.lastTransactionKey;
    target.originalBlindingFactor = source.originalBlindingFactor;
    target.originalCommitment = source.originalCommitment;
    target.source = source.source;
    target.validated = source.validated;
  }

  private getUserDetails(setOnlyCompromised: boolean = false) {
    const that = this;
    this.userService.getUserDetails(this.accountId).subscribe(r => {
      console.log(r);
      that.isCompromised = r.isCompromised;
      console.log("that.isAutoTheftProtection = " + that.isAutoTheftProtection);
      that.isAutoTheftProtection = r.isAutoTheftProtection;
      console.log("that.isAutoTheftProtection = " + that.isAutoTheftProtection);

      if (!setOnlyCompromised) {
        that.spendKey = r.publicSpendKey;
        that.viewKey = r.publicViewKey;
        that.pageTitle = r.accountInfo;
        that.qrContent = r.publicSpendKey + r.publicViewKey;
        that.isLoaded = true;
      }

      that.initializeConsentHub(r.consentManagementHub);
      //setTimeout(() => this.getUserDetails(true), 1000);
    });
  }

  private initializeUserAssociatedAttributes(userAttribute: UserAttribute) {
    this.userService.getUserAssociatedAttributes(this.accountId, userAttribute.issuer).subscribe(r => {
      userAttribute.attrPhoto = r.find(a => a.schemeName === "PassportPhoto");
      userAttribute.userAssociatedAttributes = r.filter(a => a.schemeName !== "PassportPhoto");
    });
  }

  private getGroupRelations() {
    this.userService.getGroupRelations(this.accountId).subscribe(r => {
      for (let groupRelation of r) {
        let userAttribute = this.getUserAttributeByIssuer(groupRelation.issuer, groupRelation.assetId);
        if (userAttribute) {
          userAttribute.groupRelations.push(groupRelation);
        }
      }
    });
	}

	private getUserRegistrations() {
		console.log("Loading registrations for consent requests receiving");
		this.userService.getUserRegistrations(this.accountId).subscribe(r => {
			for (const reg of r) {
				this.subscribeForConsentRequest(reg);
			}
		});
	}

	private subscribeForConsentRequest(reg: UserRegistration) {
        console.log("Registering for consent request obtaining for registration " + reg.commitment);
        this.consentHub.invoke("AddToGroup", reg.commitment).catch(e => {
            console.log(e);
        });
    }

  ProvideProofs(publicSpendKey: string, publicViewKey: string, sessionKey: string, withKnowledgeProof: boolean, withBiometricProof: boolean) {
    this.router.navigate(['/relationProofs'], { queryParams: { publicSpendKey: publicSpendKey, publicViewKey: publicViewKey, sessionKey: sessionKey, withKnowledgeProof: withKnowledgeProof, withBiometricProof: withBiometricProof } });
  }

  ValidateDocumentSignature(actionInfo: string) {
    console.log("ValidateDocumentSignature: " + actionInfo);
    let parts = actionInfo.split('.');
    let publicKey = parts[0];
    let documentHash = parts[1];
    let documentRecordHeight = parts[2];
    let signatureRecordHeight = parts[3];

    this.userService.getDocumentSignatureVerification(publicKey, documentHash, documentRecordHeight, signatureRecordHeight).subscribe(r => {
      let ver = r as DocumentSignatureVerification;
      console.log(ver);
      const dialogRef = this.dialog.open(SignatureVerificationPopup, { data: r });
      dialogRef.afterClosed().subscribe(r => { });
    });
  }

  ServiceProvider(actionInfo: string) {
    this.router.navigate(['/service-provider'], { queryParams: { actionInfo: actionInfo } });
  }

  OverrideAccount(actionInfo: string) {
    this.router.navigate(['/override-account'], { queryParams: { actionInfo } });
  }

  onSubmitAssociatedAttributes(userAttribute: UserAttribute) {

    this.userService.updateUserAssociatedAttributes(this.accountId, userAttribute.issuer, userAttribute.userAssociatedAttributes)
      .pipe(first())
      .subscribe(
        data => {
        },
        error => {
          this.error = error;
        });
  }

  onqrCodeReaderinputChanged(evt) {
    if (evt.target.value != null) {
      evt.target.value = null;
    }
  }

  onPaste(evt: any) {
    var qrCode: string;

    qrCode = evt.clipboardData.getData('text/plain');
    this.isActionError = false;
    this.actionError = "";

    if (qrCode.length >= 32) {
      this.processActionType(qrCode);
    }

    evt.stopPropagation();
  }

  processActionType(qrCode: string) {
    console.log(qrCode);
    var that = this;
    this.userService.getActionType(qrCode).subscribe(r => {
      that.action = r.action;
      that.target = r.actionInfo;

		if (r.action === ActionType.IdentityRequest) { that.router.navigate(['/userIdentityRequest'], { queryParams: { actionType: r.action, target: r.actionInfo } }); }
		else if (r.action === ActionType.ServiceProvider) { that.ServiceProvider(r.actionInfo); }
		else if (r.action === ActionType.ValidateSignature) { that.ValidateDocumentSignature(r.actionInfo); }
		else if (r.action === ActionType.ProofRelations) { that.initiateRelationProofsSession(r.actionInfo); }
		else if (r.action === ActionType.RegisterIdentity) { that.router.navigate(['/userIdentityRegistration'], { queryParams: { target: r.actionInfo } }); }
		else if (r.action === ActionType.OverrideAccount) { that.OverrideAccount(r.actionInfo); }
    }, e => {
      (<HTMLInputElement>document.getElementById("qrCodeReader")).value = "";
      that.isActionError = true;
      that.actionError = e;
    });
  }

  initiateRelationProofsSession(actionInfo: string) {
    const key: string = actionInfo;
    const dialogRef = this.dialog.open(ProofsRequestDialog, { data: { withKnowledgeProof: false, withBiometricProof: false } });
    dialogRef.afterClosed().subscribe(r => {
      console.log(r);
      if (r) {
        this.userService.challengeProofs(key, r).subscribe(r1 => {
          console.log(r1);
          this.consentHub.invoke("AddToGroup", r1.sessionKey).catch(e => {
            console.log(e);
          });
        });
      }
    });
  }

  sendCompromisedProofs() {
    this.userService.sendCompromisedProofs(this.accountId, this.unauthorizedUse).pipe(first()).subscribe(data => { }, error => { });
  }

  sanitize(img: string) {
    return this.sanitizer.bypassSecurityTrustUrl(img);
  }

  scanSuccessHandler(qrCode: string) {
    if (qrCode.length >= 32) {
      this.activateQrScanner = false;
      this.processActionType(qrCode);
    }
  }

  camerasFoundHandler(cameras: MediaDeviceInfo[]) {
    let found = false;

    this.devices = cameras;
    this.selectedDevice = 0;
    for (let cameraInfo of cameras) {
      this.selectedDevice++;
      if (cameraInfo.label.includes("back")) {
        this.device = cameraInfo;
        found = true;
      }
    }
    this.selectedDevice--;

    if (!found) {
      this.device = cameras[cameras.length - 1];
      this.selectedDevice = cameras.length - 1;
    }
  }

  switchCamera() {
    console.log(this.selectedDevice);
    this.selectedDevice = (this.selectedDevice + 1) % this.devices.length;
    console.log(this.selectedDevice);
    console.log(this.device);
    this.device = this.devices[this.selectedDevice];
    console.log(this.device);
  }

	resetCompromisedAccount() {
		let that = this;
		var dialogRef = this.dialog.open(PasswordConfirmDialog, { data: { title: "Confirm account reset", confirmButtonText: "Confirm Reset" } });
		dialogRef.afterClosed().subscribe(r => {
      if (r) {
        that.userService.resetCompromisedAccount(this.accountId, r).subscribe(
					r1 => {
						that.unauthorizedUse = null;
						that.isCompromised = false;
						that.userAttributes = [];
						alert("Account was resetted successfully. Pay attention that all Root Attributes were erased and you need to request them from Identity Providers");
					},
					e => {
						//that.dialog.open(MatAlertDialog, {
						//  data: { message: 'Failed to disclose secrets', title: 'Disclose Secrets Failure', icon: ' ' }
						//});
						alert('Failed to reset compromised account');
					});
			}
		});
  }

  onShowQrClick() {
    const key = Guid.create().toString();
    this.consentHub.invoke("AddToGroup", key);
    this.qrCodeSheetRef = this.bottomSheet.open(QrCodePopupComponent, { data: { qrCode: btoa("prf://" + key) } });
    this.qrCodeSheetRef.afterDismissed().subscribe(r => {
      this.qrCodeSheetRef = null;
    });
  }

  removeGroupRelation(userAttribute: UserAttribute, groupRelation: GroupRelation) {
    if (confirm("Are you sure you want to remove the relation to group '" + groupRelation.groupName + "' of '" + groupRelation.groupOwnerName + "'?")) {
      this.userService.deleteGroupRelation(groupRelation.groupRelationId).subscribe(r => {
        this.removeItemFromGroupRelations(userAttribute.groupRelations, groupRelation);
      });
    }
  }

  removeItemFromGroupRelations(list: GroupRelation[], itemToRemove: GroupRelation) {
    let index = -1;
    let found = false;
    for (let item of list) {
      index++;
      if (item.groupRelationId == itemToRemove.groupRelationId) {
        found = true;
        break;
      }
    }

    if (found) {
      list.splice(index, 1);
    }
  }

  deleteUserAttribute(userAttribute: UserAttribute, attributeId: number) {
    this.userService.deleteUserAttribute(this.accountId, attributeId)
      .subscribe(
        d => {
          const index = userAttribute.rootAttributes.findIndex(a => a.userAttributeId === attributeId);
          userAttribute.rootAttributes.splice(index);

          if (userAttribute.rootAttributes.length === 0) {
            const id = this.userAttributes.findIndex(a => a.assetId === userAttribute.assetId && a.issuer == userAttribute.issuer);
            this.userAttributes.splice(id);
          }
        },
        e => {
          console.log(e);
        });
  }

  getUserAttributeByIssuer(issuer: string, assetId: string) {
    for (const userAttribute of this.userAttributes) {
      if (userAttribute.issuer === issuer && userAttribute.assetId === assetId) {
        return userAttribute;
      }
    }
  }

  getAttributeValueType(schemeName: string) {
    const schemeItem = this.schemeItems.find(s => s.name === schemeName);

    if (schemeItem) {
      return schemeItem.valueType;
    }

    return undefined;
  }

  getRootAggregatedStatus(userAttribute: UserAttribute) {
    var status: number = 0; //0 - not confirmed, 1 - overriden, 2 - confirmed

    for (let rootAttr of userAttribute.rootAttributes) {
      if (rootAttr.isOverriden && rootAttr.lastCommitment !== "0000000000000000000000000000000000000000000000000000000000000000") {
        status = 1;
      }
      else if (!rootAttr.isOverriden && rootAttr.lastCommitment !== "0000000000000000000000000000000000000000000000000000000000000000") {
        return 2;
      }
    }

    return status;
  }

  onDiscloseSecrets() {
    let that = this;
	  var dialogRef = this.dialog.open(PasswordConfirmDialog, { data: { confirmButtonText: "Confirm Disclose" } });
    dialogRef.afterClosed().subscribe(r => {
      if (r) {
        that.userService.getDisclosedSecrets(this.accountId, r).subscribe(
          r1 => {
            that.bottomSheet.open(QrCodePopupComponent, { data: { qrCode: r1.qr } });
          },
          e => {
            //that.dialog.open(MatAlertDialog, {
            //  data: { message: 'Failed to disclose secrets', title: 'Disclose Secrets Failure', icon: ' ' }
            //});
            alert('Failed to disclose secrets');
          });
      }
    });
  }

	onSetUserRootAttributeContent(identity: UserAttribute, userAttribute: UserAttributeDto) {
	  userAttribute.content = userAttribute.dirtyContent;
		this.userService.setUserRootAttributeContent(this.accountId, userAttribute)
			.subscribe(
				r => {
					identity.content = userAttribute.content;
				},
				e => {
					userAttribute.content = null;
				});
	}

	stopEmulator() {
    this.cookieService.delete("accountId");
    const that = this;
    this.userService.stopAccount(this.accountId)
      .subscribe(r => {
        that.router.navigate(['/accounts']);
      });
	}
}

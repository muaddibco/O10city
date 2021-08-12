import { Component, Inject, OnInit} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { SpActionType, UserService, UserAttributeTransferWithValidationsDto, UserAttributeDto, UserAttributeTransferDto, ServiceProviderActionAndValidations } from '../user.Service';
import { HubConnection } from '@aspnet/signalr';
import { Title } from '@angular/platform-browser';
import { WebcamImage, WebcamInitError, WebcamUtil } from 'ngx-webcam';
import { Subject, Observable } from 'rxjs';
import { MatRadioChange } from '@angular/material/radio';

@Component({
	selector: 'service-provider-login-reg',
	templateUrl: './service-provider.component.html',
	styleUrls: ['./service-provider.component.css']
})

/** serviceProviderLoginReg component*/
export class ServiceProviderLoginRegComponent implements OnInit {

	public spActionTypeDocumentSign: string;
	public spActionTypeConsent: string;

	public userAttributes: UserAttributeDto[];
	public selectedAttributes: UserAttributeDto[];
	public selectedAttribute: UserAttributeDto;
	public publicKeyText: string;
	public sessionKeyText: string;
	public hubConnection: HubConnection;
	public isBusy = false;

	public validations: UserAttributeTransferWithValidationsDto;
	public withValidations: boolean = false;
	public validationEntries: string[];
	public allValidationEntries: string[];

	public documentName: string;
	public documentHash: string;
	public documentRecordHeight: string;
	public selectedGroupEntry: string;

	public accountId: number;
	public submitted = false;
  public submitClick = false;
  public isError: boolean = false;
  public errorMsg = '';

	public allowCameraSwitch = true;
	public multipleWebcamsAvailable = false;
	public isCameraAvailable = false;
	public deviceId: string;
	public errors: WebcamInitError[] = [];
	public isErrorCam: boolean;
	public errorMsgCam: string;
	public toggleCameraText: string;
	public imageContent: string;
	// latest snapshot
	public webcamImage: WebcamImage = null;

	// webcam snapshot trigger
	private trigger: Subject<void> = new Subject<void>();
	// switch to next / previous / specific webcam; true/false: forward/backwards, string: deviceId
	private nextWebcam: Subject<boolean | string> = new Subject<boolean | string>();

	public relationAttributes: string[] = [];
	public action: string;
	private actionInfo: string;
	private actionType: string = '';
	public isBiometricMandatory = false;
	public target: string;
	public target2: string;
	public sessionKey: string;
	public extraInfo: string;
	public isRegistered: boolean = false;
	public predefinedRootAttributeId: number = 0;
	public password: string;

	public isActionInfoLoaded = false;
	public isAttributesLoaded = false;

	/** serviceProviderLoginReg ctor */
	constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string, private route: ActivatedRoute, private userService: UserService, private router: Router, titleService: Title) {
		let tokenInfo = JSON.parse(sessionStorage.getItem('TokenInfo'));
		this.accountId = tokenInfo.accountId;
		this.action = 'Submit';
		this.validations = new UserAttributeTransferWithValidationsDto();
		this.validations.userAttributeTransfer = new UserAttributeTransferDto();
		titleService.setTitle(tokenInfo.accountInfo);

		this.toggleCameraText = "Turn camera off";
		this.spActionTypeDocumentSign = SpActionType.DocumentSign;
		this.spActionTypeConsent = SpActionType.Consent;
	}

	async ngOnInit() {
		this.actionInfo = this.route.snapshot.queryParams['actionInfo'];

    this.actionType = (await this.userService.getServiceProviderActionType(this.actionInfo).toPromise()).actionType;

		var that = this;
		if (this.actionType === SpActionType.DocumentSign) {
      this.userService.getServiceProviderActionInfo(that.accountId, that.actionInfo, null, null).subscribe(
				r => {
					that.processActionInfo(r);
					that.isActionInfoLoaded = true;
					that.getUserAttributes();
				},
				e => {
				}
			);
		} else if (this.actionType === SpActionType.Consent) {
			this.userService.getServiceProviderActionInfo(that.accountId, that.actionInfo, null, null).subscribe(
				r => {
					that.processActionInfo(r);
					that.isActionInfoLoaded = true;
					that.getUserAttributes();
				},
				e => {
				}
			);
		} else {
			this.isActionInfoLoaded = true;
			this.getUserAttributes();
		}

		this.initCam();
	}

	private initCam() {
		WebcamUtil.getAvailableVideoInputs()
			.then((mediaDevices: MediaDeviceInfo[]) => {
				this.multipleWebcamsAvailable = mediaDevices && mediaDevices.length > 1;
				this.isCameraAvailable = mediaDevices && mediaDevices.length > 0;
			});
	}

	private processActionInfo(actionInfo: ServiceProviderActionAndValidations) {

		this.target = actionInfo.publicKey;
		this.target2 = actionInfo.publicKey2;
		this.sessionKey = actionInfo.sessionKey;
		this.extraInfo = actionInfo.extraInfo;
		this.isRegistered = actionInfo.isRegistered;
		this.isBiometricMandatory = actionInfo.isBiomteryRequired;
		this.validationEntries = actionInfo.validations;
		this.withValidations = this.validationEntries && this.validationEntries.length > 0;

		if (this.actionType === SpActionType.Consent) {
			this.predefinedRootAttributeId = actionInfo.predefinedAttributeId;
		}

		if (this.actionType === SpActionType.DocumentSign) {
			this.documentName = this.extraInfo.split("|")[0];
			this.documentHash = this.extraInfo.split("|")[1];
			this.documentRecordHeight = this.extraInfo.split("|")[2];
			this.allValidationEntries = [];
			for (let val of actionInfo.validations) {
				if (val) {
					let parts: string[] = val.split('|');
					this.allValidationEntries.push(val);
					this.relationAttributes.push(parts[2]);
				}
			}
		}

	}

	private getUserAttributes() {
		const that = this;
    this.userService.getUserAttributes(this.accountId)
			.subscribe(r => {
				that.userAttributes = [];
				for (let item of r) {
					if (that.isAttributeEnabled(item) && item.lastCommitment != "0000000000000000000000000000000000000000000000000000000000000000") {
						that.userAttributes.push(item);
					}
				}

				if (that.userAttributes.length === 1) {
					that.selectedAttribute = that.userAttributes[0];
					that.processUserAttributeSelection(that.selectedAttribute);
				} else if (that.actionType === SpActionType.Consent) {
					that.selectedAttribute = that.userAttributes.find(a => a.userAttributeId === that.predefinedRootAttributeId);
				}

				that.isAttributesLoaded = true;
			});
	}

	private processCameraCapture() {
		if (this.isBiometricMandatory) {
			if (this.webcamImage == null) {
				this.errorMsgCam = "No image captured!";
				this.isErrorCam = true;
			}
			else {
				this.imageContent = this.webcamImage.imageAsBase64;
			}
		}
		else {
			this.imageContent = null;
		}
	}

  private authenticate() {
    this.httpClient.post<boolean>(this.baseUrl + 'User/SendIdentityProofs?accountId=' + this.accountId, this.validations).
			subscribe(r => {
				this.router.navigate(['/user']);
      },
        e => {
          this.processError(e);
});
	}

	private employeeRequest() {
		this.httpClient.post<boolean>(this.baseUrl + 'User/SendEmployeeRequest?accountId=' + this.accountId, {
			assetId: this.selectedAttribute.assetId,
			schemeName: this.selectedAttribute.schemeName,
			content: this.selectedAttribute.content,
			isOverriden: this.selectedAttribute.isOverriden,
			lastBlindingFactor: this.selectedAttribute.lastBlindingFactor,
			lastCommitment: this.selectedAttribute.lastCommitment,
			lastDestinationKey: this.selectedAttribute.lastDestinationKey,
			lastTransactionKey: this.selectedAttribute.lastTransactionKey,
			originalBlindingFactor: this.selectedAttribute.originalBlindingFactor,
			originalCommitment: this.selectedAttribute.originalCommitment,
			validated: this.selectedAttribute.validated,
			source: this.selectedAttribute.source,
			target: this.target,
			payload: this.sessionKey,
			extraInfo: this.extraInfo,
			imageContent: this.imageContent,
			password: this.password
		}).
			subscribe(r => {
				if (r) {
					this.router.navigate(['/user']);
				}
      },
        e => {
          this.processError(e);
});
	}

  private documentSignRequest() {
    this.httpClient.post<boolean>(this.baseUrl + 'User/SendDocumentSignRequest?accountId=' + this.accountId, {
      assetId: this.selectedAttribute.assetId,
      schemeName: this.selectedAttribute.schemeName,
      content: this.selectedAttribute.content,
      isOverriden: this.selectedAttribute.isOverriden,
      lastBlindingFactor: this.selectedAttribute.lastBlindingFactor,
      lastCommitment: this.selectedAttribute.lastCommitment,
      lastDestinationKey: this.selectedAttribute.lastDestinationKey,
      lastTransactionKey: this.selectedAttribute.lastTransactionKey,
      originalBlindingFactor: this.selectedAttribute.originalBlindingFactor,
      originalCommitment: this.selectedAttribute.originalCommitment,
      validated: this.selectedAttribute.validated,
      source: this.selectedAttribute.source,
      target: this.target,
      payload: this.sessionKey,
      extraInfo: this.selectedGroupEntry.replace(";", "|") + "|" + this.documentHash + "|" + this.documentRecordHeight,
      imageContent: this.imageContent,
      password: this.password
    }).
      subscribe(
        r => {
          if (r) {
            this.router.navigate(['/user']);
          }
        },
        e => {
          this.processError(e);
        });
  }

	onSubmit() {
		this.submitted = true;

		this.submitClick = true;

		this.processCameraCapture();

		if (this.actionType === SpActionType.Consent) {
			this.extraInfo = ''; // TODO: ugly patch because of SAML IdP
			this.sessionKey = this.sessionKey.split('|')[0];
		}

		this.setUserAttributeTransfer();

		if (this.actionType === SpActionType.Relation) {
			this.employeeRequest();
		}
		else if (this.actionType === SpActionType.DocumentSign) {
			this.documentSignRequest();
		}
		else if (this.actionType === SpActionType.LoginRegister || this.actionType === SpActionType.Saml || this.actionType === SpActionType.Consent) {
			this.authenticate();
		}
	}

  onCancel() {
    this.router.navigate(['/user']);
  }

	onConsentDecline() {
		this.submitted = true;

		this.submitClick = true;

		this.extraInfo = ''; // TODO: ugly patch because of SAML IdP
		this.sessionKey = this.sessionKey.split('|')[1];

		this.setUserAttributeTransfer();

		this.authenticate();
	}
     
	private setUserAttributeTransfer() {
		this.validations.userAttributeTransfer = new UserAttributeTransferDto();
		this.validations.userAttributeTransfer.assetId = this.selectedAttribute.assetId;
		this.validations.userAttributeTransfer.schemeName = this.selectedAttribute.schemeName;
		this.validations.userAttributeTransfer.content = this.selectedAttribute.content;
		this.validations.userAttributeTransfer.isOverriden = this.selectedAttribute.isOverriden;
		this.validations.userAttributeTransfer.lastBlindingFactor = this.selectedAttribute.lastBlindingFactor;
		this.validations.userAttributeTransfer.lastCommitment = this.selectedAttribute.lastCommitment;
		this.validations.userAttributeTransfer.lastDestinationKey = this.selectedAttribute.lastDestinationKey;
		this.validations.userAttributeTransfer.lastTransactionKey = this.selectedAttribute.lastTransactionKey;
		this.validations.userAttributeTransfer.originalBlindingFactor = this.selectedAttribute.originalBlindingFactor;
		this.validations.userAttributeTransfer.originalCommitment = this.selectedAttribute.originalCommitment;
		this.validations.userAttributeTransfer.validated = this.selectedAttribute.validated;
		this.validations.userAttributeTransfer.source = this.selectedAttribute.source;
		this.validations.userAttributeTransfer.target = this.target;
		this.validations.userAttributeTransfer.target2 = this.target2;
		this.validations.userAttributeTransfer.payload = this.sessionKey;
		this.validations.userAttributeTransfer.extraInfo = this.extraInfo;
		this.validations.userAttributeTransfer.password = this.password;
		this.validations.userAttributeTransfer.imageContent = this.imageContent;
		this.validations.password = this.password;
	}

	public triggerSnapshot(): void {
		this.trigger.next();
	}

	public showNextWebcam(directionOrDeviceId: boolean | string): void {
		this.nextWebcam.next(directionOrDeviceId);
	}

	public handleImage(webcamImage: WebcamImage): void {
		console.info('received webcam image', webcamImage);
		this.webcamImage = webcamImage;
	}

	public handleInitError(error: WebcamInitError): void {
		this.errors.push(error);
	}

	public cameraWasSwitched(deviceId: string): void {
		console.log('active device: ' + deviceId);
		this.deviceId = deviceId;
	}

	public get triggerObservable(): Observable<void> {
		return this.trigger.asObservable();
	}

	public get nextWebcamObservable(): Observable<boolean | string> {
		return this.nextWebcam.asObservable();
	}

	clearSnapshot() {
		this.webcamImage = null;
	}

	onAttributeSelected(evt: MatRadioChange) {
		let userAttribute: UserAttributeDto = evt.value;

		this.processUserAttributeSelection(userAttribute);
	}

	private processUserAttributeSelection(userAttribute: UserAttributeDto) {
		if (this.actionType === SpActionType.DocumentSign) {
			this.validationEntries = [];
			for (let item of this.allValidationEntries) {
				if (item.endsWith('|' + userAttribute.source + ';' + userAttribute.assetId)) {
					this.validationEntries.push(item.split('|')[0] + '|' + item.split('|')[1]);
				}
			}
			this.withValidations = this.validationEntries.length > 0;
			if (this.validationEntries.length === 1) {
				this.selectedGroupEntry = this.validationEntries[0].split('|')[0];
			}
		} else {
      this.isBusy = true;
      this.userService.getServiceProviderActionInfo(this.accountId, this.actionInfo, userAttribute.assetId, userAttribute.content)
				.subscribe(r => {
					this.processActionInfo(r);
					this.isBusy = false;
				}, err => {
            this.isBusy = false;
            this.processError(err);
				});
		}
	}

	isAttributeEnabled(attribute: UserAttributeDto) {
		if (this.actionType === SpActionType.DocumentSign) {
			return this.relationAttributes.findIndex(a => a === attribute.source + ';' + attribute.assetId) >= 0;
		}

		return true;
	}

  private processError(err: any) {
    this.isError = true;
    if (err && err.error && err.error.message) {
      this.errorMsg = err.error.message;
    } else if (err && err.error) {
      this.errorMsg = err.error;
    } else {
      this.errorMsg = err;
    }
  }
}

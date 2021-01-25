import { Component, OnInit, QueryList } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { GroupRelation, UserService, UserAttributeDto } from '../user.Service';
import { WebcamImage, WebcamInitError, WebcamUtil } from 'ngx-webcam';
import { Subject, Observable } from 'rxjs';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { MatListOption } from '@angular/material/list';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { FormControl, Validators } from '@angular/forms';

interface GroupRelationSelectable extends GroupRelation {
  isSelected: boolean;
}


@Component({
  templateUrl: 'group-relations-proof.component.html',
})
export class GroupRelationsProofComponent implements OnInit {
  private accountId: number;
  public relations: GroupRelationSelectable[];
  public targetSpendKey: string;
  public targetViewKey: string;
	public sessionKey: string;
	public withKnowledgeProof: string;
	public withBiometricProof: string;
	public password: string;
	passwordInput = new FormControl('', [Validators.required]);
	public isStepperLastStep: boolean = false;

  public userAttributes: UserAttributeDto[];
  public selectedAttribute: UserAttributeDto;

  public showWebcam = true;
  public allowCameraSwitch = true;
  public multipleWebcamsAvailable = false;
  public isCameraAvailable = false;
  public deviceId: string;
  public errors: WebcamInitError[] = [];
  public isErrorCam: boolean;
  public errorMsgCam: string;
  public skipCamera = false;
  public imageContent: string;
  // latest snapshot
  public webcamImage: WebcamImage = null;

  // webcam snapshot trigger
  private trigger: Subject<void> = new Subject<void>();
  // switch to next / previous / specific webcam; true/false: forward/backwards, string: deviceId
  private nextWebcam: Subject<boolean | string> = new Subject<boolean | string>();


  constructor(private userService: UserService, private router: Router, private route: ActivatedRoute, titleService: Title) {
    let tokenInfo = JSON.parse(sessionStorage.getItem('TokenInfo'));
    this.accountId = tokenInfo.accountId;
    titleService.setTitle("Group Relations Proofs");
    this.relations = [];
  }

  ngOnInit() {
	  this.targetSpendKey = this.route.snapshot.queryParams['publicSpendKey'];
	  this.targetViewKey = this.route.snapshot.queryParams['publicViewKey'];
	  this.sessionKey = this.route.snapshot.queryParams['sessionKey'];
	  this.withKnowledgeProof = this.route.snapshot.queryParams['withKnowledgeProof'];
	  this.withBiometricProof = this.route.snapshot.queryParams['withBiometricProof'];

	  this.getUserAttributes();

	  if (this.withBiometricProof) {
		  this.initCam();
	  }

    this.userService.getGroupRelations(this.accountId).subscribe(r => {
      this.relations = r.map<GroupRelationSelectable>(g => { return { ...g, isSelected: false }; });
    });
  }

  private initCam() {
    WebcamUtil.getAvailableVideoInputs()
      .then((mediaDevices: MediaDeviceInfo[]) => {
        this.multipleWebcamsAvailable = mediaDevices && mediaDevices.length > 1;
        this.isCameraAvailable = mediaDevices && mediaDevices.length > 0;
        this.skipCamera = !mediaDevices || mediaDevices.length == 0;
      });
  }

	private getUserAttributes() {
    this.userService.
      getUserAttributes(this.accountId)
			.subscribe(r => {
				this.userAttributes = [];
				for (let item of r) {
					if (!item.isOverriden) {
						this.userAttributes.push(item);
					}
				}

				if (this.userAttributes.length === 1) {
					this.selectedAttribute = this.userAttributes[0];
				}
			});
	}

  public triggerSnapshot(): void {
    this.trigger.next();
  }

  public toggleWebcam(): void {
    this.showWebcam = !this.showWebcam;
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

  onCancel() {
    this.router.navigate(['/user']);
  }

  onRelationSelectionChanged(evt: MatCheckboxChange, relation: GroupRelationSelectable) {
    relation.isSelected = evt.checked;
  }

  onSubmit() {
    console.log(this.relations);
    let selectedRelations: GroupRelation[] = this.relations.filter(v => {
      if (v.isSelected) {
        return v;
      }
    })

    console.log(selectedRelations);

    this.userService.sendRelationProofs(this.accountId, {
		  withBiometricProof: this.withBiometricProof === 'true',
      withKnowledgeProof: this.withKnowledgeProof === 'true',
		  imageContent: (this.withBiometricProof === 'true' && this.isCameraAvailable && this.showWebcam) ? this.webcamImage.imageAsBase64 : null,
      target: this.targetSpendKey,
      target2: this.targetViewKey,
		  relations: selectedRelations,
      password: this.password,
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
		  payload: this.sessionKey
    })
      .subscribe(r => {
        this.router.navigate(['/user']);
      });
	}

	onStepperSelectionChanged(evt: StepperSelectionEvent) {
		let extraSteps = this.withBiometricProof == "true" ? 1 : 0;
		if (this.withKnowledgeProof == "true") {
			extraSteps++;
		}

		if (this.userAttributes.length === 1) {
			extraSteps--;
		}
		this.isStepperLastStep = evt.selectedIndex == 1 + extraSteps;
	}
}

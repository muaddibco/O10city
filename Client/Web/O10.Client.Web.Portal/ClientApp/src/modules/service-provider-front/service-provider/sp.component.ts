import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ServiceProviderFrontService, SpDocument, SpDocumentSignature } from '../service-provider-front.service';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@aspnet/signalr';
import { Title } from '@angular/platform-browser';
import { MatDialog } from '@angular/material/dialog';
import { NotificationPopupDialog } from 'src/modules/notification-popup/notification-popup.component';

interface SpDocumentWithQR {
  model: SpDocument,
  qrContent: string,
  signatures: SpDocumentSignatureWithQR[]
}

interface SpDocumentSignatureWithQR {
  model: SpDocumentSignature,
  qrContent: string
}

@Component({
	templateUrl: './sp.component.html'
})

export class SpComponent implements OnInit {

	public pageTitle: string;
	public isLoaded: boolean;
	public isDocumentsLoaded: boolean = false;
	public publicKey: string;
	public sessionKey: string;
	public hubConnection: HubConnection;
	private oldSessionKey: string;
	public isRegistered: boolean;
	public isError: boolean;
	public isAlreadyRegistered: boolean;
	public isAuthenticationFailed: boolean;
	public isVerificationFailed: boolean;
	public verificationErrorText: string;
	public qrCodeContent: string;
	public qrCode2Content: string;
	public isQrGenerated: boolean;
	public documents: SpDocumentWithQR[];
	hubConnected = false;

	constructor(@Inject('BASE_URL') private baseUrl: string, private route: ActivatedRoute, private router: Router, private serviceProviderService: ServiceProviderFrontService, public dialog: MatDialog, private titleService: Title) {
		this.isLoaded = false;
		this.oldSessionKey = null;
		this.isRegistered = false;
		this.isAlreadyRegistered = false;
		this.isAuthenticationFailed = false;
		this.isVerificationFailed = false;
		this.isError = false;
		this.isQrGenerated = false;
		this.documents = [];
	}

	ngOnInit() {
		this.hubConnection = new HubConnectionBuilder().withUrl("/identitiesHub").build();

		this.hubConnection.on("PushUserRegistration", (i) => {
			console.log("PushUserRegistration");
			const dialogRef = this.dialog.open(NotificationPopupDialog, { data: { title: "Registration Succeeded", messages: ["Congratulations!", "You just registered to our service!", "Now you can proceed to login for starting using it"] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

		this.hubConnection.on("PushUserAlreadyRegistered", (i) => {
			console.log("PushUserAlreadyRegistered");
			const dialogRef = this.dialog.open(NotificationPopupDialog, { role: 'alertdialog', data: { title: "Already Registered", messages: ["You already have account at our service!", "Now you can proceed to login for starting using it"] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

		this.hubConnection.on("PushEmployeeNotRegistered", (i) => {
			console.log("PushEmployeeNotRegistered");
			const dialogRef = this.dialog.open(NotificationPopupDialog, { role: 'alertdialog', data: { title: "Relation Registration", messages: ["Failed to register relation"] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

		this.hubConnection.on("PushEmployeeIncorrectRegistration", (i) => {
			console.log("PushEmployeeIncorrectRegistration: " + i.message);
			const dialogRef = this.dialog.open(NotificationPopupDialog, { role: 'alertdialog', data: { title: "Relation Registration", messages: ["Failed to register relation", i.message] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

		this.hubConnection.on("PushEmployeeRegistration", (i) => {
			console.log("PushEmployeeRegistration");
			const dialogRef = this.dialog.open(NotificationPopupDialog, { data: { title: "Relation Registration", messages: ["Relation registration succeeded with Registration Commitment:", i.commitment] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

		this.hubConnection.on("PushRelationAlreadyRegistered", (i) => {
			console.log("PushRelationAlreadyRegistered");
			sessionStorage.setItem('TokenInfo', JSON.stringify(i));
			sessionStorage.setItem('spId', this.route.snapshot.paramMap.get('id'));
			sessionStorage.setItem('sessionKey', this.sessionKey);
			sessionStorage.setItem('byRelation', "true");
			this.router.navigate(['/spLoginHome']);
			//const dialogRef = this.dialog.open(NotificationPopupDialog, { data: { title: "Relation Registration", messages: ["Relation already exist", "Thank you for checking"] } });
			//dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

		this.hubConnection.on("PushDocumentSignIncorrect", (i) => {
			console.log("PushDocumentSignIncorrect");
			const dialogRef = this.dialog.open(NotificationPopupDialog, { role: 'alertdialog', data: { title: "Document Signature", messages: ["Failed to sign document", i.message] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

		this.hubConnection.on("PushDocumentSignature", (i) => {
			const signature = i as SpDocumentSignature;
			console.log("PushDocumentSignature");
			console.log(this.documents);
			console.log(signature);
			const document = this.documents.find(d => d.model.documentId === signature.documentId);
			document.model.signatures.push(signature);
			const signatureWithQr = this.getSpDocumentSignatureWithQR(this.publicKey, signature);
			document.signatures.push(signatureWithQr);
			const dialogRef = this.dialog.open(NotificationPopupDialog, { data: { title: "Document Signature", messages: ["Document " + document.model.documentName + " signed successfully", i.commitment] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

		this.hubConnection.on("PushSpAuthorizationSucceeded", (i) => {
			sessionStorage.setItem('TokenInfo', JSON.stringify(i));
			sessionStorage.setItem('spId', this.route.snapshot.paramMap.get('id'));
			sessionStorage.setItem('sessionKey', this.sessionKey);
			sessionStorage.setItem('byRelation', "false");
			this.router.navigate(['/spLoginHome']);
		});

		this.hubConnection.on("PushSpAuthorizationFailed", (i) => {
			console.log("PushSpAuthorizationFailed: " + i.message);
			const dialogRef = this.dialog.open(NotificationPopupDialog, { role: 'alertdialog', data: { title: "User Authorization", messages: ["User authorization failed", i.message] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

    this.hubConnection.on("EligibilityCheckFailed", (i) => {
      console.log("EligibilityCheckFailed");
			const dialogRef = this.dialog.open(NotificationPopupDialog, { role: 'alertdialog', data: { title: "User Authorization", messages: ["User authorization failed", "Eligibility Proofs were wrong"] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

    this.hubConnection.on("ProtectionCheckFailed", (i) => {
      console.log("ProtectionCheckFailed: " + i);
			const dialogRef = this.dialog.open(NotificationPopupDialog, { role: 'alertdialog', data: { title: "User Authorization", messages: ["User authorization failed", "Knowledge Proofs were wrong", i] } });
			dialogRef.afterClosed().subscribe(r => { this.onReset(); });
		});

		this.initializeSession();
	}

	initializeSession() {
		var that = this;
		this.serviceProviderService.getServiceProvider(this.route.snapshot.paramMap.get('id')).subscribe(r => {
			that.pageTitle = r.description;
			that.titleService.setTitle(r.description + " Portal")

			this.serviceProviderService.getSessionInfo(this.route.snapshot.paramMap.get('id')).subscribe(r => {
				that.publicKey = r.publicKey;
				that.sessionKey = r.sessionKey;
				that.qrCodeContent = btoa("spp://" + that.baseUrl + "SpUsers/Action?t=0&pk=" + r.publicKey + "&sk=" + r.sessionKey);
				that.qrCode2Content = btoa("spp://" + that.baseUrl + "SpUsers/Action?t=1&pk=" + r.publicKey + "&sk=" + r.sessionKey);
				console.log("qrCodeContent: " + that.qrCodeContent);
				console.log("qrCode2Content: " + that.qrCode2Content);
				that.isRegistered = false;
				that.isAlreadyRegistered = false;
				that.isQrGenerated = true;
				that.isLoaded = true;

				that.serviceProviderService.getSpDocuments(that.route.snapshot.paramMap.get('id')).subscribe(r => {
					that.documents = [];
					for (let doc of r) {
						that.documents.push({
							model: doc,
							qrContent: btoa("spp://" + that.baseUrl + "SpUsers/Action?t=2&pk=" + that.publicKey + "&sk=" + that.sessionKey + "&rk=" + doc.hash),
							signatures: doc.signatures.map<SpDocumentSignatureWithQR>(s => {
								return that.getSpDocumentSignatureWithQR(that.publicKey, s);
							})
						})
					}
					that.isDocumentsLoaded = true;
				});

				if (that.oldSessionKey != null) {
					that.hubConnection.invoke("RemoveFromGroup", that.oldSessionKey);
					that.hubConnection.invoke("AddToGroup", that.sessionKey);
					that.oldSessionKey = that.sessionKey;
				}
				else {

					this.hubConnection.onclose(e => {
						console.log("hubConnection.onclose: [" + e.name + "] " + e.message);
						this.startHubConnection(that);
					});

					this.startHubConnection(that);
				}
			});
		});
	}

	private startHubConnection(that: this) {
		that.hubConnection.start()
			.then(() => {
				console.log("IdentityHub Connection started");
				that.hubConnection.invoke("AddToGroup", that.sessionKey);
				that.oldSessionKey = that.sessionKey;
			})
			.catch(err => {
				console.error(err);
				setTimeout(() => that.startHubConnection(that), 1000);
			});
	}

	onReset() {
		this.isAlreadyRegistered = false;
		this.isRegistered = false;
		this.isError = false;
		this.isVerificationFailed = false;
		this.isAuthenticationFailed = false;
		this.verificationErrorText = "";
		this.isQrGenerated = false;
		this.qrCodeContent = "";
		this.isLoaded = false;

		this.initializeSession();
	}

	getSpDocumentSignatureWithQR(publicKey: string, s: SpDocumentSignature) {
		return { model: s, qrContent: btoa("sig://" + publicKey + "." + s.documentHash + "." + s.documentRecordHeight.toString() + "." + s.signatureRecordHeight.toString()) }
	}
}

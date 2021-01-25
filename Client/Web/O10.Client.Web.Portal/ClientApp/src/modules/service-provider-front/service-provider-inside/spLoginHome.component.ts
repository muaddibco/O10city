import { Component, OnInit } from '@angular/core';
import { ServiceProviderFrontService, SpDocument, AllowedSigner, SpDocumentSignature, ServiceProviderRelationGroups, ServiceProviderRegistrationDto } from '../service-provider-front.service';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { MatDialog } from '@angular/material/dialog';
import { Title } from '@angular/platform-browser';
import { AllowedSignerDialog } from '../allowed-signer-dialog/allowed-signer-dialog.component'
import { DocumentDialog } from '../document-dialog/document-dialog.component'
import { AddTransactionDialog } from '../add-transaction-dialog/add-transaction.dialog';

@Component({
	templateUrl: './spLoginHome.component.html'
})

export class SpLoginHomeComponent implements OnInit {

	public isLoaded: boolean;
	public pageTitle: string;
	public spId: string;
	public isCompromised: boolean;
	public areDocumentsLoaded: boolean = false;
	public hubConnection: HubConnection;
	public registrationTransactions: ServiceProviderRegistrationDto[];
	public spRelationGroups: ServiceProviderRelationGroups[];
	public documents: SpDocument[] = [];
	public byRelation: boolean = false;
	public areRegistrationTransactionsLoaded: boolean = false;

	constructor(private serviceProviderService: ServiceProviderFrontService, private dialog: MatDialog, private titleService: Title) {
		console.log("SpLoginHomeComponent constructor");
		this.isLoaded = false;
		this.spId = sessionStorage.getItem('spId');
		this.byRelation = sessionStorage.getItem('byRelation') === 'true';
		this.isCompromised = false;

		console.log("SpLoginHomeComponent constructor; spId = " + this.spId);
	}

	ngOnInit() {
		console.log("SpLoginHomeComponent ngOnInit");

		const that = this;
		this.configureSignalR();

		this.serviceProviderService.getServiceProvider(this.spId).subscribe(r => {
			console.log("SpLoginHomeComponent ngOnInit; getServiceProvider.r.description = " + r.description);
			this.pageTitle = r.description;
			this.titleService.setTitle(r.description + " - Account page");
			this.isLoaded = true;
		});

		if (this.byRelation) {
			this.serviceProviderService.getRegistrationsAndTransactions(this.spId).subscribe((r) => {
				that.registrationTransactions = r;
				for (const r of that.registrationTransactions) {
					if (!r.transactions) {
						r.transactions = [];
					}
				}
				that.areRegistrationTransactionsLoaded = true;
			});
		}

		this.serviceProviderService.getSpDocumentSignatures(this.spId).subscribe((r) => {
			this.documents = r;
			for (let doc of this.documents) {
				if (!doc.allowedSigners) {
					doc.allowedSigners = [];
				}
			}
			this.areDocumentsLoaded = true;
		});

		this.serviceProviderService.getServiceProviderRelationGroups().subscribe(r => {
			this.spRelationGroups = r;
		});
	}

	private configureSignalR() {
		this.hubConnection = new HubConnectionBuilder().withUrl("/identitiesHub").build();
		this.hubConnection.on("PushAuthorizationCompromised", (i) => {
			this.isCompromised = true;
		});
		this.hubConnection.onclose(e => {
			console.log("hubConnection.onclose: [" + e.name + "] " + e.message);
			this.startHubConnection();
		});
		this.startHubConnection();
	}

	private startHubConnection() {
		this.hubConnection.start()
			.then(() => {
				console.log("IdentityHub Connection started");
				this.hubConnection.invoke("AddToGroup", sessionStorage.getItem('sessionKey'));
			})
			.catch(err => { console.error(err); });
	}

	openAddNewDocumentDialog() {
		const dialogRef = this.dialog.open(DocumentDialog, { width: '400px' });

		const that = this;
		dialogRef.afterClosed().subscribe(result => {
			if (result) {
				that.serviceProviderService.addSpDocument(this.spId, { documentId: 0, documentName: result.documentName, hash: result.hash, allowedSigners: null, signatures: null }).subscribe((r) => {
					r.allowedSigners = [];
					that.documents.push(r);
				});
			}
		});
	}

	openAddAllowedSignerDialog(doc: SpDocument) {
		const dialogRef = this.dialog.open(AllowedSignerDialog, { width: '400px', data: { documentId: doc.documentId, spRelationGroups: this.spRelationGroups } });

		const that = this;
		dialogRef.afterClosed().subscribe(result => {
			if (result) {
				that.serviceProviderService.addAllowedSigner(this.spId, result.documentId, { allowedSignerId: 0, groupOwner: result.selectedServiceProvider.publicSpendKey, groupName: result.selectedRelationGroup.name }).subscribe((r) => {
					for (let doc of that.documents) {
						if (doc.documentId === result.documentId) {
							doc.allowedSigners.push(r);
						}
					}
				});
			}
		});
	}

	removeAllowedSigner(doc: SpDocument, allowedSigner: AllowedSigner) {
		const that = this;
		if (confirm("Are you sure you want to remove Allowed Signer " + allowedSigner.groupOwner + " : " + allowedSigner.groupName + "?")) {
			this.serviceProviderService.deleteAllowedSigner(this.spId, allowedSigner.allowedSignerId).subscribe(r => {
				that.removeAllowedSignerFromList(doc.allowedSigners, allowedSigner);
			});
		}
	}

	removeAllowedSignerFromList(list: AllowedSigner[], itemToRemove: AllowedSigner) {
		let index = -1;
		let found = false;
		for (let item of list) {
			index++;
			if (item.allowedSignerId == itemToRemove.allowedSignerId) {
				found = true;
				break;
			}
		}

		if (found) {
			list.splice(index, 1);
		}
	}

	onAddTransaction(registrationId: string) {
		const dialogRef = this.dialog.open(AddTransactionDialog, {
			width: '400px',
			data: { registration: this.registrationTransactions.find(r => r.serviceProviderRegistrationId === registrationId).commitment }
		});

		dialogRef.afterClosed().subscribe(r => {
			this.serviceProviderService
				.pushUserTransaction(this.spId, registrationId, r.description)
				.subscribe(r => {
					this.registrationTransactions.find(reg => reg.serviceProviderRegistrationId === r.registrationId.toString()).transactions.push(r);
				});
		});
	}
}

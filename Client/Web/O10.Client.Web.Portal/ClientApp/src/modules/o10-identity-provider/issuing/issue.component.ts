import { Component, Inject, OnInit } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@aspnet/signalr';
import { O10IdentityProviderService } from '../o10-identity-provider.services';
import { NotificationPopupDialog } from '../../notification-popup/notification-popup.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
	templateUrl: './issue.component.html'
})
export class IssueComponent implements OnInit {
	public isLoaded = false;
	public sessionSucceeded = false;
	public sessionFailed = false;
	public qrCode: string;
	public hubConnection: HubConnection;
	private sessionKey: string;

  constructor(private service: O10IdentityProviderService, private dialog: MatDialog) {
	}

	ngOnInit() {
		this.hubConnection = new HubConnectionBuilder().withUrl("/idpNotifications").build();

		this.hubConnection.on("AttributeIssued", (i) => {
          console.log("AttributeIssued");
          this.dialog.open(NotificationPopupDialog, { data: { title: "Attribute (Re)Issuing", messages: ["Requested Attributed was sent to your wallet"], btnName: "OK" } });
		});


    this.hubConnection.on("AttributeIssueFailed", (i) => {
      console.log("AttributeIssueFailed: " + i);
      this.dialog.open(NotificationPopupDialog, { data: { title: "Attribute (Re)Issuing", messages: ["Requested Attributed sending failed", i], btnName: "OK" } });
    });

		this.service.getIssuingSessionInfo().subscribe(r => {
			this.qrCode = r.uri;
			this.sessionKey = r.sessionKey;

			this.startHubConnection(this);

			this.isLoaded = true;
		});
	}

	private startHubConnection(that: this) {
		that.hubConnection.start()
			.then(() => {
				console.log("idpNotifications Connection started");
				that.hubConnection.invoke("AddToGroup", that.sessionKey);
			})
			.catch(err => {
				console.error(err);
				setTimeout(() => that.startHubConnection(that), 1000);
			});
	}
}

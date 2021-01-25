import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { O10IdentityProviderService } from '../o10-identity-provider.services'
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@aspnet/signalr';
import { NotificationPopupDialog } from '../../notification-popup/notification-popup.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  templateUrl: './registration-confirmation.component.html',
  styleUrls: ['./registration-confirmation.component.scss']
})
export class RegistrationConfirmationComponent implements OnInit {

  public qrCode: string;
  public isLoaded = false;
  public hubConnection: HubConnection;
  private sessionKey: string;

  constructor(private service: O10IdentityProviderService, private route: ActivatedRoute, private dialog: MatDialog, @Inject("BASE_URL") private baseUrl: string) {

  }

  ngOnInit() {
    this.hubConnection = new HubConnectionBuilder().withUrl("/idpNotifications").build();

    this.hubConnection.on("AttributeRegistered", (i) => {
      console.log("AttributeIssued");
      this.dialog.open(NotificationPopupDialog, { data: { title: "Attribute Registration", messages: ["Requested Attributed was registered and sent to your wallet"], btnName: "OK" } });
    });

    this.startHubConnection(this);

    this.hubConnection.on("AttributeRegistrationFailed", (i) => {
      console.log("AttributeIssueFailed: " + i);
      this.dialog.open(NotificationPopupDialog, { data: { title: "Attribute Registration", messages: ["Requested Attributed registration failed", i], btnName: "OK" } });
    });


    this.sessionKey = this.route.snapshot.queryParams['sk'];
	  this.qrCode = btoa("wreg://" + this.baseUrl + "IdentityProvider/RegistrationDetails/" + this.sessionKey);
    this.isLoaded = true;
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

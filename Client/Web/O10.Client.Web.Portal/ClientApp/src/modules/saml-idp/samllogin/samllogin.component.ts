import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { SamlIdpService, SamlIdpSessionResponse } from '../saml-idp.services';
import { Router, ActivatedRoute } from '@angular/router';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@aspnet/signalr';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
	templateUrl: './samllogin.component.html',
	styleUrls: ['./samllogin.component.scss']
})
export class SamlLoginComponent implements OnInit {

  public hubConnection: HubConnection;
  public isLoaded = false;
  public qrCode: string;
  public sessionKey: string;

  public samlRedirectUri: string;
  public samlResponse: string;
  public relayState: string;

  @ViewChild('samlForm') samlFormElement;

  constructor(private service: SamlIdpService, private route: ActivatedRoute) {

  }

  ngOnInit() {
	  const samlRequest = this.route.snapshot.queryParams['SAMLRequest'];
	  const relayState = this.route.snapshot.queryParams['RelayState'];

    this.hubConnection = new HubConnectionBuilder().withUrl("/samlIdpHub").build();

    this.hubConnection.on('SamlIdpSessionResponse', args => {
      const response: SamlIdpSessionResponse = args as SamlIdpSessionResponse;

      console.log(response);

      
      this.samlRedirectUri = response.redirectUri;
      this.samlResponse = response.saml2Response.response;
      this.relayState = response.saml2Response.relayState;
      this.samlFormElement.nativeElement.action = response.redirectUri;
      (<HTMLInputElement>document.getElementById("samlResponse")).value = response.saml2Response.response;
      (<HTMLInputElement>document.getElementById("relayState")).value = response.saml2Response.relayState;
      this.samlFormElement.nativeElement.submit();
    });

    const that = this;

	  this.service.initiateSamlSession(encodeURIComponent(samlRequest), relayState ? encodeURIComponent(relayState) : null).subscribe(r => {
      that.qrCode = btoa('saml://' + r.sessionInfo);
      this.sessionKey = r.sessionKey;

      that.hubConnection.onclose(e => {
        console.log("hubConnection.onclose: [" + e.name + "] " + e.message);
        this.startHubConnection(that);
      });

      that.startHubConnection(that);

      that.isLoaded = true;
    });
  }

  private startHubConnection(that: this) {
    that.hubConnection.start()
      .then(() => {
        console.log("SamlIdpHub Connection started");
        that.hubConnection.invoke("AddToGroup", that.sessionKey);
      })
      .catch(err => {
        console.error(err);
        setTimeout(() => that.startHubConnection(that), 1000);
      });
  }
}

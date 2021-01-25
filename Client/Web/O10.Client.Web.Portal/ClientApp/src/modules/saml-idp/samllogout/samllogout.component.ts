import { Component, OnInit, ViewChild } from '@angular/core';
import { SamlIdpService } from '../saml-idp.services';
import { ActivatedRoute } from '@angular/router';

@Component({
	templateUrl: './samllogout.component.html',
	styleUrls: ['./samllogout.component.scss']
})
export class SamlLogoutComponent implements OnInit {

  public isLoaded = false;
  public qrCode: string;
  public sessionKey: string;

  public samlRedirectUri: string;
  public samlResponse: string;
  public relayState: string;

  @ViewChild('samlForm') samlFormElement;

  constructor(private service: SamlIdpService, private route: ActivatedRoute) { }

  ngOnInit() {
	  const samlRequest = this.route.snapshot.queryParams['SAMLRequest'];
	  const relayState = this.route.snapshot.queryParams['RelayState'];

    const that = this;

	  this.service.logout(encodeURIComponent(samlRequest), relayState ? encodeURIComponent(relayState) : null).subscribe(r => {
		  console.log(r);

		  that.samlFormElement.nativeElement.action = r.redirectUri + '?SAMLResponse=' + r.saml2Response.response + '&Signature=' + r.signature + '&SigAlg=' + r.signatureAlgorithm;
		  //(<HTMLInputElement>document.getElementById("samlResponse")).value = r.saml2Response.response;
		  //(<HTMLInputElement>document.getElementById("relayState")).value = r.saml2Response.relayState;
		  that.samlFormElement.nativeElement.submit();

      that.isLoaded = true;
    });
  }
}

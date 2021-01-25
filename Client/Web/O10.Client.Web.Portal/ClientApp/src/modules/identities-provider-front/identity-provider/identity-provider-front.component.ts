import { Component, OnInit, Inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { IdentitiesProviderFrontService } from '../identities-provider-front.service';

@Component({
  templateUrl: './identity-provider-front.component.html',
})

export class IdentityProviderFrontComponent implements OnInit {

  public isLoaded = false;
  public title: string;
  public qrCode: string;

  constructor(
    private service: IdentitiesProviderFrontService,
    @Inject('BASE_URL') private baseUrl: string,
    private route: ActivatedRoute,
    private titleService: Title) { }

  ngOnInit() {
    let accountId = this.route.snapshot.paramMap.get('id');
	  this.service.getIdentityProvider(accountId).subscribe(r => {
		  this.titleService.setTitle(r.description);
		  this.title = r.description;
		  this.qrCode = btoa("iss://" + this.baseUrl + "IdentityProvider/IssuanceDetails?issuer=" + r.target);
		  this.isLoaded = true;
	  });
  }
}

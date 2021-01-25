import { Component, OnInit } from '@angular/core';
import { IdentityProviderInfoDto, IdentitiesProviderFrontService } from '../identities-provider-front.service';
import { AuthorizeService } from 'src/api-authorization/authorize.service';
import { ScenariosService } from '../../scenarios/scenarios.service';



@Component({
  templateUrl: './identity-providers-list.component.html',
})

export class IdentityProvidersListComponent implements OnInit {

  public isLoaded = false;
	public identityProviders: IdentityProviderInfoDto[];
	private isInScenario: boolean = false;

	constructor(private service: IdentitiesProviderFrontService, private authorizeService: AuthorizeService, private scenarioService: ScenariosService) {
    this.identityProviders = [];
  }

  ngOnInit() {
	  this.authorizeService.getUser().subscribe(async u => {
		  if (u && u.role === "Admin") {
			  this.service.getIdentityProviders().subscribe(r => {
				  this.identityProviders = r;
				  this.isLoaded = true;
			  });
		  } else {
			  const scenarioId = await this.scenarioService.fetchActiveScenarioId();
			  if (scenarioId !== "0") {
				  this.service.getIdentityProvidersForSession(scenarioId).subscribe(r => {
					  this.identityProviders = r;
					  this.isLoaded = true;
				  });
			  } else {
				  this.identityProviders = [];
				  this.isLoaded = true;
			  }
		  }
	  });
  }
}

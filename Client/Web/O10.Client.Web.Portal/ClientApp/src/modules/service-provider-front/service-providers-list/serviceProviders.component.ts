import { Component, OnInit } from '@angular/core';
import { ServiceProviderInfoDto, ServiceProviderFrontService } from '../service-provider-front.service';
import { AuthorizeService } from 'src/api-authorization/authorize.service';
import { Title } from '@angular/platform-browser';
import { ScenariosService } from '../../scenarios/scenarios.service';

@Component({
  templateUrl: './serviceProviders.component.html'
})

export class ServiceProvidersComponent implements OnInit {
  public serviceProviders: ServiceProviderInfoDto[];
  public isLoaded: boolean;

	constructor(private serviceProviderService: ServiceProviderFrontService, private authorizeService: AuthorizeService, private scenarioService: ScenariosService, titleService: Title) {
    this.isLoaded = false;
    titleService.setTitle("O10 Demo Portal - Service Providers");
  }

	ngOnInit() {
		this.authorizeService.getUser().subscribe(async u => {
			if (u && u.role === "Admin") {
				this.serviceProviderService.getServiceProviders().subscribe(r => {
					this.serviceProviders = r;
					this.isLoaded = true;
				});
			} else {
				const scenarioId = await this.scenarioService.fetchActiveScenarioId();
				if (scenarioId !== "0") {
					this.serviceProviderService.getServiceProvidersForSession(scenarioId).subscribe(r => {
						this.serviceProviders = r;
						this.isLoaded = true;
					});
				} else {
					this.serviceProviders = [];
					this.isLoaded = true;
				}
			}
		});
	}
}

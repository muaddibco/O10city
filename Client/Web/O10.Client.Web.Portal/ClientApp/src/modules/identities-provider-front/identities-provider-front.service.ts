import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class IdentitiesProviderFrontService {

  constructor(private http: HttpClient) { }

  getIdentityProviders() {
    return this.http.get<IdentityProviderInfoDto[]>('/api/IdentityProvider/All');
  }

	getIdentityProvidersForSession(scenarioId: string) {
		return this.http.get<IdentityProviderInfoDto[]>('/api/IdentityProvider/All?scenarioId=' + scenarioId);
	}

  getIdentityProvider(accountId: string) {
    return this.http.get<IdentityProviderInfoDto>('/api/IdentityProvider/ById/' + accountId);
  }
}

export interface IdentityProviderInfoDto {
  id: string;
  description: string;
  target: string;
}

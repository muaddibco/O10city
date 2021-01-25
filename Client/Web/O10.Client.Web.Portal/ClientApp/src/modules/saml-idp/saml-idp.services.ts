import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";

@Injectable()
export class SamlIdpService {
  constructor(private http: HttpClient) {

  }

  initiateSamlSession(samlRequest: string, relayState: string) {
	  return this.http.get<SamlSession>("/api/SamlIdp/InitiateSamlSession?samlRequest=" + samlRequest + (relayState? '&relayState=' + relayState : ''));
	}

	logout(samlRequest: string, relayState: string) {
		return this.http.get<SamlIdpSessionResponse>('/api/SamlIdp/Logout?samlRequest=' + samlRequest + (relayState ? '&relayState=' + relayState : ''))
	}
}

export interface SamlSession {
  sessionInfo: string;
  sessionKey: string;
}

export interface Saml2Response {
  response: string;
  relayState: string;
}

export interface SamlIdpSessionResponse {
  sessionId: string;
  redirectUri: string;
	saml2Response: Saml2Response;
	signature: string;
	signatureAlgorithm: string;
}

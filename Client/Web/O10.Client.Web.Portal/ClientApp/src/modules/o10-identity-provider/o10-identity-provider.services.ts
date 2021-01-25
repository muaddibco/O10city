import { Injectable, Inject } from '@angular/core';
import { HttpClient } from "@angular/common/http";

@Injectable()
export class O10IdentityProviderService {
  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {

  }

  sendRegistrationMail(email: string, passphrase: string) {
    return this.http.post<any>("/api/IdentityProvider/RegisterWithEmail", { email: encodeURIComponent(email), passphrase: encodeURIComponent(passphrase), baseUri: encodeURIComponent(this.baseUrl)})
	}

	getIssuingSessionInfo() {
		return this.http.get<IIssuingSessionInfo>("/api/IdentityProvider/IssueSessionData");
	}

  getRegistrationConfirmationSession() {

  }
}

interface IIssuingSessionInfo {
	sessionKey: string;
	uri: string;
}

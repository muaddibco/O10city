import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { map } from 'rxjs/operators';

const header = new HttpHeaders()
  .set('Content-type', 'application/json');

@Injectable()
export class AuthenticationService {
  constructor(private http: HttpClient) { }

  login(accountId: number, password: string) {
    return this.http.post<AccountDto>('/accounts/authenticate', { accountId, password })
      .pipe(map(user => {
        // login successful if there's a jwt token in the response
        if (user) {
          // store user details and jwt token in local storage to keep user logged in between page refreshes
          sessionStorage.setItem('TokenInfo', JSON.stringify(user));
          sessionStorage.setItem('AccountType', JSON.stringify(user.accountType));
          sessionStorage.setItem('PublicSpendKey', JSON.stringify(user.publicSpendKey));
          sessionStorage.setItem('PublicViewKey', JSON.stringify(user.publicViewKey));
          sessionStorage.setItem('AccountId', JSON.stringify(user.accountId));
        }
        return user;
      }));
  }

  register(accountType: number, accountInfo: string, password: string) {
    const body = JSON.stringify({ accountInfo, password, accountType });
    return this.http.post<AccountDto>('/accounts/register', { accountInfo, password, accountType }); //, { headers: httpHeaders, responseType: "json" });
  }
}

export interface AccountDto {
	accountId: number;
	accountType: number;
	accountInfo: string;
	password: string;
	token: string;
	publicViewKey: string;
	publicSpendKey: string;
	showLogin: boolean;
	error: string;
	isError: boolean;
}

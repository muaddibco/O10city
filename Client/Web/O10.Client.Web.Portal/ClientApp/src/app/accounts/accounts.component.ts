import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AccountDto } from 'src/modules/authentication/authentication.service';
import { AuthorizeService } from 'src/api-authorization/authorize.service'
import { ScenariosService } from '../../modules/scenarios/scenarios.service';

@Component({
	selector: 'app-accounts',
	templateUrl: './accounts.component.html'
})
export class AccountsComponent implements OnInit {
	public accounts: AccountDto[];
	public accountsUser: AccountDto[];
	public accountsSP: AccountDto[];
	public accountsIP: AccountDto[];
	public isLoaded: boolean;
	public showUserOnly = false;
	public isInScenario = false;

	constructor(
		private service: AuthorizeService,
    private scenarioService: ScenariosService,
		private http: HttpClient,
		@Inject('BASE_URL') private baseUrl: string,
		private router: Router,
		titleService: Title) {
		this.isLoaded = false;
		this.accountsUser = [];
		this.accountsSP = [];
		this.accountsIP = [];
		titleService.setTitle("O10 Demo Portal - Accounts");
	}

	ngOnInit() {
		var ua = navigator.userAgent;
		if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua)) {
			this.showUserOnly = true;
		}

		this.service.getUser().subscribe(async u => {
			if (u && u.role === "Admin") {
				this.isInScenario = false;
			} else {
				this.isInScenario = true;
				this.showUserOnly = true;
			}

			let scenarioId = await this.scenarioService.fetchActiveScenarioId();

			if (this.isInScenario) {
				this.http.get<AccountDto[]>(this.baseUrl + 'Accounts/GetAll?scenarioId=' + scenarioId).subscribe(result => {
					this.buildAccounts(result);
					this.isLoaded = true;
				}, error => console.error(error));
			} else {
				this.http.get<AccountDto[]>(this.baseUrl + 'Accounts/GetAll').subscribe(result => {
					this.buildAccounts(result);
					this.isLoaded = true;
				}, error => console.error(error));
			}
		});
	}

	private buildAccounts(result: AccountDto[]) {
		this.accountsIP = [];
		this.accountsSP = [];
		this.accountsUser = [];
		this.accounts = result;
		for (var a of result) {
			a.showLogin = false;
			if (a.accountType == 1) {
				this.accountsIP.push(a);
			}
			if (a.accountType == 2) {
				this.accountsSP.push(a);
			}
			if (a.accountType == 3) {
				this.accountsUser.push(a);
			}
		}
	}

	gotoLogin(account: AccountDto) {
		this.router.navigate(['/login', account.accountId], { queryParams: { returnUrl: this.router.url } });
	}

	removeIPAccount(account: AccountDto) {
		if (confirm("Are you sure you want to remove the Account " + account.accountInfo + "?")) {
			this.http.post<any>(this.baseUrl + 'Accounts/RemoveAccount', account).subscribe(r => {
				this.removeAccountFromList(this.accountsIP, account);
			});
		}
	}

	removeSPAccount(account: AccountDto) {
		if (confirm("Are you sure you want to remove the Account " + account.accountInfo + "?")) {
			this.http.post<any>(this.baseUrl + 'Accounts/RemoveAccount', account).subscribe(r => {
				this.removeAccountFromList(this.accountsSP, account);
			});
		}
	}

	removeUserAccount(account: AccountDto) {
		if (confirm("Are you sure you want to remove the Account " + account.accountInfo + "?")) {
			this.http.post<any>(this.baseUrl + 'Accounts/RemoveAccount', account).subscribe(r => {
				this.removeAccountFromList(this.accountsUser, account);
			});
		}
	}

	removeAccountFromList(list: AccountDto[], itemToRemove: AccountDto) {
		let index = -1;
		let found = false;
		for (let item of list) {
			index++;
			if (item.accountId == itemToRemove.accountId) {
				found = true;
				break;
			}
		}

		if (found) {
			list.splice(index, 1);
		}
	}

	navigateToRegister() {
		this.router.navigate(['/register']);
	}
}

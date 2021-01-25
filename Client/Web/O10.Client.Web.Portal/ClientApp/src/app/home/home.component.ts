import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { AuthorizeService, IUser } from 'src/api-authorization/authorize.service'
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {
	private user: IUser;
  public isAuthenticated: Observable<boolean>;
  public userName: Observable<string>;

  constructor(titleService: Title, private router: Router, private authorizeService: AuthorizeService, private cookieService: CookieService) {
    titleService.setTitle("O10 Demo Portal Home");
	}

	ngOnInit() {
    this.isAuthenticated = this.authorizeService.isAuthenticated();
    this.userName = this.authorizeService.getUser().pipe(map(u => u && u.name));
		var ua = navigator.userAgent;
		if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua)) {
			if (this.cookieService.check("accountId")) {
				let accountId = this.cookieService.get("accountId");
				this.router.navigate(['/login', accountId], { queryParams: { returnUrl: '/accounts' } });
			}
			else {
				this.router.navigate(['/accounts']);
			}
		}
	}
}

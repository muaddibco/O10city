import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

	public showNavMenu = false;
	public isMobile = false;

	constructor() {

  }
  title = 'app';

	ngOnInit() {
    var ua = navigator.userAgent;
		if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua)) {
			this.showNavMenu = false;
			this.isMobile = true;
    }
    else {
      this.showNavMenu = true;
			this.isMobile = false;
    }
  }
}

import { Component, OnInit } from '@angular/core';
import { AuthorizeService } from 'src/api-authorization/authorize.service'

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})

export class NavMenuComponent implements OnInit {
  isExpanded = false;
  isHidden: boolean;
	isAdmin: boolean = false;
	isScenarioActive: boolean = false;
	isAuthenticated: boolean = false;

  constructor(private service: AuthorizeService) { this.isHidden = true; }

	ngOnInit() {
		this.isScenarioActive = localStorage.getItem('scenarioId') !== null;
		this.service.getUser().subscribe(u => {
			if (u) {
				this.isAuthenticated = true;
				if (u.role === "Admin") {
					this.isAdmin = true;
				}
			}
		});
	}

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}

import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ScenariosService, Scenario } from '../scenarios.service';

@Component({
	templateUrl: './scenario-list.component.html',
	styleUrls: ['./scenario-list.component.scss']
})
export class ScenarioListComponent implements OnInit {

  public isLoaded: boolean = false;
	public scenarios: Scenario[];

  constructor(private service: ScenariosService, private router: Router) { }

	ngOnInit() {
		this.service.getScenarios().subscribe(r => {
      this.scenarios = r;
      this.isLoaded = true;
		});
  }

  async gotoScenario(scenario: Scenario) {
    const currentScenarioId = await this.service.fetchActiveScenarioId();
    if (currentScenarioId && currentScenarioId !== "0" && currentScenarioId !== scenario.id) {
      if (confirm('There is another scenario in progress. Do you want to abandone it and start new scenario?')) {
        this.service.removeScenarioSession(currentScenarioId).subscribe(s => {
          this.router.navigate(['/scenario', scenario.id]);
        });
      }
    } else {
      this.router.navigate(['/scenario', scenario.id]);
    }
  }
}

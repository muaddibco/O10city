import { Component, OnInit } from '@angular/core';
import { ScenariosService, Scenario } from '../scenarios.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  templateUrl: './scenario-playback.component.html',
  styleUrls: ['./scenario-playback.component.scss'],
})
export class ScenarioPlaybackComponent implements OnInit {

	public scenario: Scenario;
	public content: string;
	public isLoading: boolean = false;

  constructor(private service: ScenariosService, private route: ActivatedRoute) { }

  async ngOnInit() {
    this.isLoading = true;
    const scenarioId = await this.service.fetchActiveScenarioId();
    if (scenarioId === "0") {
      this.startNewScenario(this.route.snapshot.paramMap.get('id'));
      this.isLoading = false;
    } else {
      this.service.getScenario(scenarioId).subscribe(s => {
        if (s.isActive) {
          this.service.resumeScenario(s.id).subscribe(r => {
            this.scenario = r;
            this.loadContent();
            this.isLoading = false;
          });
        } else {
          this.startNewScenario(scenarioId);
        }
      });
    }
	}

  private startNewScenario(scenarioId: string) {
	  this.service.startNewScenario(scenarioId).subscribe(s => {
		  this.scenario = s;
      this.loadContent();
	  });
  }

  private loadContent() {
    this.service.getStepContent(this.scenario.id).subscribe(s => { this.content = s.content; this.isLoading = false; });
  }

	onNextStep() {
		this.service.nextStep(this.scenario.id).subscribe(s => {
			this.scenario.currentStep++;
			this.content = s.content;
		});
	}

	onPrevStep() {
		this.service.prevStep(this.scenario.id).subscribe(s => {
			this.scenario.currentStep--;
			this.content = s.content;
		});
	}

	reset() {
		if (confirm("Are you sure you want to reset this scenario?")) {
			this.isLoading = true;
			this.service.removeScenarioSession(this.scenario.id).subscribe(r => {
				this.startNewScenario(this.scenario.id);
			});
		}
  }
}

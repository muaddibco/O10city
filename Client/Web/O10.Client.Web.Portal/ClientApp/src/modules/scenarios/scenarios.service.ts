import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class ScenariosService {
	constructor(private http: HttpClient) { }

	getScenarios() {
		return this.http.get<Scenario[]>('/api/Scenarios');
	}

	getActiveSession() {
		return this.http.get<Scenario>('/api/Scenarios/ActiveScenario');
	}

  getScenario(scenarionId: string) {
    return this.http.get<Scenario>('/api/Scenarios/' + scenarionId);
  }

  removeScenarioSession(scenarionId: string) {
    return this.http.delete('/api/Scenarios/' + scenarionId);
  }

  startNewScenario(scenarioId: string) {
    return this.http.put<Scenario>('/api/Scenarios/' + scenarioId, null);
  }

  resumeScenario(scenarioId: string) {
    return this.http.post<Scenario>('/api/Scenarios/' + scenarioId, null);
  }

  getStepContent(scenarioId: string) {
    return this.http.get<StepContent>('/api/Scenarios/' + scenarioId + '/api/Step');
  }

  nextStep(scenarioId: string) {
    return this.http.post<StepContent>('/api/Scenarios/' + scenarioId + '/api/Step?forward=true', null);
  }

  prevStep(scenarioId: string) {
	  return this.http.post<StepContent>('/api/Scenarios/' + scenarioId + '/api/Step?forward=false', null);
	}

  async fetchActiveScenarioId() {
    const r = await this.getActiveSession().toPromise();
    if (r) {
      return r.id.toString();
    } else {
      return "0";
    }
  }
}

export interface Scenario {
	id: string;
	name: string;
	isActive: boolean;
	sessionId: number;
	currentStep: number;
  startTime: Date;
  steps: ScenarioStep[];
}

export interface StepContent {
  content: string;
}

export interface ScenarioStep {
  id: number;
  caption: string;
}

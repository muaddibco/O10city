import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatStepperModule } from '@angular/material/stepper';

import { ScenariosService } from './scenarios.service'
import { ScenarioListComponent } from './scenarios-list/scenario-list.component'
import { ScenarioPlaybackComponent } from './scenario-playback/scenario-playback.component'

import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { MarkdownModule } from 'ngx-markdown'

@NgModule({
  declarations: [ScenarioListComponent, ScenarioPlaybackComponent],
  imports: [
    BrowserModule,
    HttpClientModule,
    MatStepperModule, MatDividerModule, MatButtonModule,
    MarkdownModule.forChild(),
    RouterModule.forRoot([
      { path: 'scenarioList', component: ScenarioListComponent, canActivate: [AuthorizeGuard] },
      { path: 'scenario/:id', component: ScenarioPlaybackComponent, canActivate: [AuthorizeGuard] }
    ])
  ],
  providers: [ScenariosService],
  bootstrap: [ScenarioListComponent, ScenarioPlaybackComponent]
})
export class ScenariosModule { }

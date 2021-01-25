import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { FormsModule, ReactiveFormsModule  } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { IssuerDetailsComponent } from './components/issuer-details/issuer-details.component';
import { IssuerDetailsListComponent } from './components/issuer-details-list/issuer-details-list.component';
import { O10IdentityService } from "./services/o10identity.service";
import { IssuerRegisterComponent } from './components/issuer-register/issuer-register.component';
import { SchemeDetailsComponent } from './components/scheme-details/scheme-details.component';

@NgModule({
  declarations: [
    AppComponent,
    IssuerDetailsComponent,
    IssuerDetailsListComponent,
    IssuerRegisterComponent,
    SchemeDetailsComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule
  ],
  providers: [O10IdentityService],
  bootstrap: [AppComponent]
})
export class AppModule { }

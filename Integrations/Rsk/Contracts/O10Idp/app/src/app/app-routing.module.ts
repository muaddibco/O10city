import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IssuerDetailsListComponent } from "./components/issuer-details-list/issuer-details-list.component";
import { IssuerRegisterComponent } from "./components/issuer-register/issuer-register.component";
import { SchemeDetailsComponent } from './components/scheme-details/scheme-details.component';

const routes: Routes = [
  { path: '', redirectTo: '/issuers', pathMatch: 'full' },
  { path: 'issuers', component: IssuerDetailsListComponent },
  { path: 'registerIssuer', component: IssuerRegisterComponent },
  { path: 'schemeDetails', component: SchemeDetailsComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

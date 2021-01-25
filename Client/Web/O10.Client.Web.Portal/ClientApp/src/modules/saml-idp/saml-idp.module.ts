import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { QrCodeExModule } from '../qrcode/qrcode.module';

import { SamlLoginComponent } from './samllogin/samllogin.component';
import { SamlLogoutComponent } from './samllogout/samllogout.component';
import { SamlIdpService } from './saml-idp.services';

@NgModule({
	declarations: [SamlLoginComponent, SamlLogoutComponent],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot([
		{ path: 'samllogin', component: SamlLoginComponent },
		{ path: 'samllogout', component: SamlLogoutComponent}
    ]),
    QrCodeExModule
  ],
  providers: [SamlIdpService],
	bootstrap: [SamlLoginComponent, SamlLogoutComponent]
})
export class SamlIdpModule { }

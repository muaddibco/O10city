import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { QrCodeExModule } from '../qrcode/qrcode.module';

import { O10IdentityProviderService } from './o10-identity-provider.services'
import { RegistrationComponent } from './registration/registration.component'
import { RegistrationConfirmationComponent } from './registration-confirmation/registration-confirmation.component'
import { IssueComponent } from './issuing/issue.component'

@NgModule({
	declarations: [RegistrationComponent, RegistrationConfirmationComponent, IssueComponent],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot([
      { path: 'idpreg', component: RegistrationComponent },
      { path: 'idpregconfirm', component: RegistrationConfirmationComponent },
		  { path: 'issue', component: IssueComponent }
    ]),
    MatInputModule, MatFormFieldModule, MatButtonModule, MatDialogModule,
    QrCodeExModule
  ],
  providers: [O10IdentityProviderService],
  exports: [RegistrationComponent, RegistrationConfirmationComponent],
	bootstrap: [RegistrationComponent, RegistrationConfirmationComponent, IssueComponent]
})
export class O10IdenitityProviderModule { }

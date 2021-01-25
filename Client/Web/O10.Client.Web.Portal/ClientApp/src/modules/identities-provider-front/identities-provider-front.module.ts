import { NgModule } from '@angular/core'
import { IdentityProvidersListComponent } from './identity-providers-list/identity-providers-list.component';
import { IdentityProviderFrontComponent } from './identity-provider/identity-provider-front.component';
import { IdentitiesProviderFrontService } from './identities-provider-front.service';
import { RouterModule } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { QrCodeExModule } from 'src/modules/qrcode/qrcode.module';
import { ScenariosModule } from 'src/modules/scenarios/scenarios.module';
import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';

@NgModule({
  declarations: [IdentityProvidersListComponent, IdentityProviderFrontComponent],
  imports: [
    BrowserModule,
    HttpClientModule,
	  QrCodeExModule,
	  ScenariosModule,
    RouterModule.forRoot([
      { path: 'identityProviders', component: IdentityProvidersListComponent, canActivate: [AuthorizeGuard] },
      { path: 'ip/:id', component: IdentityProviderFrontComponent }
    ])
  ],
  providers: [IdentitiesProviderFrontService],
  exports: [IdentityProvidersListComponent, IdentityProviderFrontComponent],
  bootstrap: [IdentityProvidersListComponent, IdentityProviderFrontComponent]
})
export class IdentitiesProviderFrontModule { }

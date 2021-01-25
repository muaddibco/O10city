import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule  } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { AccountsComponent } from './accounts/accounts.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { portalReducer } from './store/portal.reducers';
import { PortalEffects } from './store/portal.effects';
import { AuthenticationModule } from 'src/modules/authentication/authentication.module';
import { IdentityProviderBackModule } from 'src/modules/identities-provider-back/identityProviderBack.module';
import { QrCodeExModule } from 'src/modules/qrcode/qrcode.module';
import { IdentitiesProviderFrontModule } from 'src/modules/identities-provider-front/identities-provider-front.module';
import { UserModule } from 'src/modules/user/user.module';
import { ServiceProviderBackModule } from 'src/modules/service-provider-back/service-provider-back.module';
import { ServiceProviderFrontModule } from 'src/modules/service-provider-front/service-provider-front.module';
import { ScenariosModule } from 'src/modules/scenarios/scenarios.module';
import { SamlIdpModule } from 'src/modules/saml-idp/saml-idp.module';
import { O10IdenitityProviderModule } from 'src/modules/o10-identity-provider/o10-identity-provider.module'

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatInputModule } from '@angular/material/input';
import { CookieService } from 'ngx-cookie-service';

import { ApiAuthorizationModule } from 'src/api-authorization/api-authorization.module';
import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';
import { MarkdownModule } from 'ngx-markdown';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    AccountsComponent,
    LoginComponent,
    RegisterComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    MatExpansionModule, MatInputModule,
	  ApiAuthorizationModule,
	  AuthenticationModule,
    QrCodeExModule,
    IdentityProviderBackModule,
    IdentitiesProviderFrontModule,
    UserModule,
    ServiceProviderBackModule,
	  ServiceProviderFrontModule,
    ScenariosModule,
	  SamlIdpModule,
	  O10IdenitityProviderModule,
    MarkdownModule.forRoot(),
    StoreModule.forRoot({ portal: portalReducer }),
    EffectsModule.forRoot([PortalEffects]),
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'accounts', component: AccountsComponent, canActivate: [AuthorizeGuard] },
      { path: 'login/:id', component: LoginComponent, canActivate: [AuthorizeGuard] },
      { path: 'register', component: RegisterComponent, canActivate: [AuthorizeGuard]},
    ])
  ],
  providers: [
    CookieService,
	  { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

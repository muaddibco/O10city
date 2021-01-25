import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { IdentitiesService } from './identities.service';
import { IdentityProviderComponent } from './identity-provider/identityProvider.component';
import { ViewUserIdentityComponent } from './view-user-identity/viewUserIdentity.component';
import { DefineSchemeComponent } from './define-scheme/define-scheme.component'
import { AddAttributeDialog } from './add-attribute-dialog/add-attribute-dialog.component'
import { AddIdentityDialog } from './add-identity-dialog/add-identity.dialog'
import { identityProviderBackReducer } from './store/identity-provider-back.reducers';
import { IdentityProviderBackEffects } from './store/identity-provider-back.effects'
import { QrCodeExModule } from 'src/modules/qrcode/qrcode.module';
import { QrCodePopupModule } from 'src/modules/qrcode-popup/qrcode-popup.module';
//import { MatDialogsModule } from '@angular-material-extensions/core';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { ApiAuthorizationModule } from 'src/api-authorization/api-authorization.module';
import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';

@NgModule({
	declarations: [IdentityProviderComponent, ViewUserIdentityComponent, DefineSchemeComponent, AddAttributeDialog, AddIdentityDialog],
	imports: [
		BrowserModule,
		HttpClientModule,
		FormsModule,
		ReactiveFormsModule,
		ApiAuthorizationModule,
		//MatDialogsModule,
		MatSlideToggleModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatSelectModule, MatInputModule, MatBottomSheetModule,
		QrCodeExModule,
		QrCodePopupModule,
		StoreModule.forRoot({ identityProviderBackEditor: identityProviderBackReducer }),
		EffectsModule.forRoot([IdentityProviderBackEffects]),
		RouterModule.forRoot([
			{ path: 'identityProvider', component: IdentityProviderComponent, canActivate: [AuthorizeGuard] },
			{ path: 'view-identity/:id', component: ViewUserIdentityComponent, canActivate: [AuthorizeGuard] },
			{ path: 'defineScheme', component: DefineSchemeComponent, canActivate: [AuthorizeGuard] },
		])
	],
	providers: [IdentitiesService],
	exports: [IdentityProviderComponent, ViewUserIdentityComponent],
	bootstrap: [IdentityProviderComponent, ViewUserIdentityComponent, DefineSchemeComponent, AddAttributeDialog, AddIdentityDialog]
})
export class IdentityProviderBackModule { }


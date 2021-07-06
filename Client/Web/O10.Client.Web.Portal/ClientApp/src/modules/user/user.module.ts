import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

//import { MatDialogsModule } from '@angular-material-extensions/core';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatStepperModule } from '@angular/material/stepper';

import { WebcamModule } from 'ngx-webcam';
import { QRCodeModule } from 'angularx-qrcode';
import { ZXingScannerModule } from '@zxing/ngx-scanner';
import { CookieService } from 'ngx-cookie-service';

import { DuplicateUserComponent } from './duplicate-user/duplicate-user.component';
import { UserService } from './user.Service';
import { UserComponent } from './user.component';
import { UserIdentityRequestComponent } from './user-identity-request/userIdentityRequest.component';
import { IdentityRegistrationComponent } from './identity-registration/identity-registration.component'
import { ServiceProviderLoginRegComponent } from './service-provider/service-provider.component';
import { QrCodeExModule } from '../qrcode/qrcode.module';
import { SignatureVerificationPopup } from './signature-verification-popup/signature-verification-popup.component';
import { QrCodePopupModule } from '../qrcode-popup/qrcode-popup.module';
import { GroupRelationsProofComponent } from './group-relations-proof/group-relations-proof.component';
import { RelationsValidationPopup } from './relations-validation-popup/relations-validation-popup.component';
import { OverrideAccountComponent } from './override-account/override-account.component';
import { PasswordConfirmDialog } from './password-confirm-dialog/password-confirm.dialog';
import { ConsentConfirmDialog } from './consent-confirm-dialog/consent-confirm.dialog';
import { ProofsRequestDialog } from './proofs-request-dialog/proofs-request.dialog';
import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';

@NgModule({
	declarations: [UserComponent, UserIdentityRequestComponent, ServiceProviderLoginRegComponent, DuplicateUserComponent, SignatureVerificationPopup, GroupRelationsProofComponent, RelationsValidationPopup, IdentityRegistrationComponent, OverrideAccountComponent, PasswordConfirmDialog, ProofsRequestDialog, ConsentConfirmDialog],
	imports: [
		BrowserModule,
		HttpClientModule,
		FormsModule,
		ReactiveFormsModule,
		WebcamModule,
		QRCodeModule,
		QrCodePopupModule,
		ZXingScannerModule,
		BrowserAnimationsModule,
		//MatDialogsModule,
		MatExpansionModule, MatInputModule, MatSelectModule, MatDialogModule, MatButtonModule, MatBottomSheetModule, MatCardModule, MatIconModule, MatProgressBarModule, MatListModule, MatButtonToggleModule, MatDividerModule, MatStepperModule, MatCheckboxModule, MatRadioModule, MatFormFieldModule, MatSlideToggleModule,
		QrCodeExModule,
		RouterModule.forRoot([
			{ path: 'user', component: UserComponent, canActivate: [AuthorizeGuard] },
			{ path: 'service-provider', component: ServiceProviderLoginRegComponent },
			{ path: 'duplicate-user', component: DuplicateUserComponent, canActivate: [AuthorizeGuard] },
			{ path: 'userIdentityRequest', component: UserIdentityRequestComponent, canActivate: [AuthorizeGuard] },
			{ path: 'userIdentityRegistration', component: IdentityRegistrationComponent, canActivate: [AuthorizeGuard] },
			{ path: 'relationProofs', component: GroupRelationsProofComponent, canActivate: [AuthorizeGuard] },
			{ path: 'override-account', component: OverrideAccountComponent, canActivate: [AuthorizeGuard] }
		])
	],
	providers: [UserService, CookieService],
	exports: [UserComponent],
	bootstrap: [UserComponent, UserIdentityRequestComponent, ServiceProviderLoginRegComponent, DuplicateUserComponent, SignatureVerificationPopup, GroupRelationsProofComponent, RelationsValidationPopup, IdentityRegistrationComponent, OverrideAccountComponent, PasswordConfirmDialog, ProofsRequestDialog, ConsentConfirmDialog]
})
export class UserModule { }

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';

import { ServiceProvidersComponent } from './service-providers-list/serviceProviders.component';
import { SpComponent } from './service-provider/sp.component';
import { SpLoginHomeComponent } from './service-provider-inside/spLoginHome.component';
import { ServiceProviderFrontService } from './service-provider-front.service';
import { QrCodeExModule } from 'src/modules/qrcode/qrcode.module';
import { NotificationPopupModule } from 'src/modules/notification-popup/notification-popup.module';
import { DocumentDialog } from './document-dialog/document-dialog.component';
import { QRCodeModule } from 'angularx-qrcode';
import { ScenariosModule } from 'src/modules/scenarios/scenarios.module';
import { AllowedSignerDialog } from './allowed-signer-dialog/allowed-signer-dialog.component'
import { AddTransactionDialog } from './add-transaction-dialog/add-transaction.dialog';

import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';

@NgModule({
	declarations: [ServiceProvidersComponent, SpComponent, SpLoginHomeComponent, DocumentDialog, AllowedSignerDialog, AddTransactionDialog],
  imports: [
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
	  QRCodeModule,
	  ScenariosModule,
    MatExpansionModule, MatInputModule, MatTabsModule, MatSelectModule, MatDialogModule, MatButtonModule, MatBottomSheetModule, MatCardModule, MatIconModule, MatDividerModule, MatProgressBarModule, MatListModule, MatGridListModule,
    QrCodeExModule,
    NotificationPopupModule,
    RouterModule.forRoot([
      { path: 'serviceProviders', component: ServiceProvidersComponent, canActivate: [AuthorizeGuard] },
      { path: 'sp/:id', component: SpComponent, canActivate: [AuthorizeGuard] },
      { path: 'spLoginHome', component: SpLoginHomeComponent, canActivate: [AuthorizeGuard] },
    ])
  ],
  providers: [ServiceProviderFrontService],
	bootstrap: [ServiceProvidersComponent, SpComponent, SpLoginHomeComponent, DocumentDialog, AllowedSignerDialog, AddTransactionDialog]
})
export class ServiceProviderFrontModule { }

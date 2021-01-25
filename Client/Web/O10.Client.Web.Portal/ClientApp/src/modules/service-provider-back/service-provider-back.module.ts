import { NgModule } from '@angular/core';
import { ServiceProviderComponent } from './serviceProvider.component';
import { QrCodeExModule } from '../qrcode/qrcode.module';
import { RouterModule } from '@angular/router';
import { ServiceProviderService } from './serviceProvider.service';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { DialogAddValidationDialog } from './add-validation-dialod/add-validation-dialog.component';
import { QrCodePopupModule } from 'src/modules/qrcode-popup/qrcode-popup.module';
import { DialogEmployeeGroupDialog } from './employee-group-dialog/employee-group-dialog.component';
import { DialogEmployeeRecordDialog } from './employee-record-dialog/employee-record-dialog.component';
import { DocumentDialog } from './document-dialog/document-dialog.component';
import { QRCodeModule } from 'angularx-qrcode';
import { AllowedSignerDialog } from './allowed-signer-dialog/allowed-signer-dialog.component';
import { AddTransactionDialog } from './add-transaction-dialog/add-transaction.dialog';

import { ApiAuthorizationModule } from 'src/api-authorization/api-authorization.module';
import { AuthorizeGuard } from 'src/api-authorization/authorize.guard';
import { AuthorizeInterceptor } from 'src/api-authorization/authorize.interceptor';

@NgModule({
	declarations: [ServiceProviderComponent, DialogAddValidationDialog, DialogEmployeeGroupDialog, DialogEmployeeRecordDialog, DocumentDialog, AllowedSignerDialog, AddTransactionDialog],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    MatExpansionModule, MatInputModule, MatTabsModule, MatSelectModule, MatDialogModule, MatButtonModule, MatBottomSheetModule, MatCardModule, MatIconModule, MatDividerModule, MatProgressBarModule, MatListModule,
    QrCodeExModule,
    QrCodePopupModule,
    QRCodeModule,
    RouterModule.forRoot([
		{ path: 'serviceProvider', component: ServiceProviderComponent, canActivate: [AuthorizeGuard] }
    ])
  ],
  providers: [ServiceProviderService],
	bootstrap: [ServiceProviderComponent, DialogAddValidationDialog, DialogEmployeeGroupDialog, DialogEmployeeRecordDialog, DocumentDialog, AllowedSignerDialog, AddTransactionDialog]
})
export class ServiceProviderBackModule { }

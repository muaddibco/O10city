import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormControl, FormGroupDirective, NgForm, Validators, FormBuilder, FormGroup } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { O10IdentityProviderService } from '../o10-identity-provider.services'
import { NotificationPopupDialog } from '../../notification-popup/notification-popup.component';
import { MatDialog } from '@angular/material/dialog';

/** Error when invalid control is dirty, touched, or submitted. */
export class MyErrorStateMatcher implements ErrorStateMatcher {
  isErrorState(control: FormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return !!(control && control.invalid && (control.dirty || control.touched || isSubmitted));
  }
}

@Component({
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.scss']
})
export class RegistrationComponent implements OnInit {
  registrationForm: FormGroup;
  emailFormControl = new FormControl('', [
    Validators.required,
    Validators.email,
  ]);
  passphraseFormControl = new FormControl('', [
    Validators.required
  ]);

  matcher = new MyErrorStateMatcher();

  constructor(private service: O10IdentityProviderService, private router: Router, public dialog: MatDialog) {

  }

  ngOnInit() {
  }

  onSubmit() {
    this.service.sendRegistrationMail(this.emailFormControl.value, this.passphraseFormControl.value)
      .subscribe(
        r => {
          const dialogRef = this.dialog.open(NotificationPopupDialog, { data: { title: "Registration Initiated", messages: ["Activation mail was sent to:", this.emailFormControl.value], btnName: "OK" } });
        },
        e => {
          const dialogRef = this.dialog.open(NotificationPopupDialog, { data: { title: "Registration Failed", messages: ["Failed to register with email", this.emailFormControl.value, e.error], btnName: "OK" } });
        });
  }

	onCancel() {
		this.router.navigate(['/']);
  }
}

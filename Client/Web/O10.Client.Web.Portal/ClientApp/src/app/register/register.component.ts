import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { Title } from '@angular/platform-browser';
import { AuthenticationService } from 'src/modules/authentication/authentication.service';

enum AccountType {
  IdentityProvider = 1,
  ServiceProvider = 2,
  User = 3
}

@Component({
  templateUrl: './register.component.html'
})

export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  submitClick = false;
  submitted = false;
  error = '';
  options: string[];
  public isIpSelected = false;
  public isSpSelected = false;
  public isUserSelected = false;

  constructor(private formBuilder: FormBuilder,private router: Router,private authenticationService: AuthenticationService,titleService: Title) {titleService.setTitle("O10 Demo Portal - Register");}

  ngOnInit() {

    this.registerForm = this.formBuilder.group({
      accountInfo: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  get formData() { return this.registerForm.controls; }

  onRegister() {
    this.submitted = true;
    if (this.registerForm.invalid) {
      return;
    }

    this.submitClick = true;

    var accountType = this.isIpSelected ? AccountType.IdentityProvider : (this.isSpSelected ? AccountType.ServiceProvider : AccountType.User);
    this.authenticationService.register(accountType, this.formData.accountInfo.value, this.formData.password.value)
      .pipe(first())
      .subscribe(
      data => {
        this.router.navigate(['/accounts']);
        },
      error => {
        this.error = error;
        this.submitClick = false;
      });
  }

  onCancel() {
    this.router.navigate(['/accounts']);
  }

  selectIP() {
    this.isIpSelected = true;
    this.isSpSelected = false;
    this.isUserSelected = false;
  }

  selectSP() {
    this.isIpSelected = false;
    this.isSpSelected = true;
    this.isUserSelected = false;
  }

  selectUser() {
    this.isIpSelected = false;
    this.isSpSelected = false;
    this.isUserSelected = true;
  }
}

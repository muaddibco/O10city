import { Component, Inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { first } from 'rxjs/operators';
import { Title } from '@angular/platform-browser';
import { AuthenticationService } from 'src/modules/authentication/authentication.service';
import { CookieService } from 'ngx-cookie-service';
import { AccountDto } from 'src/modules/authentication/authentication.service';

@Component({
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})

export class LoginComponent implements OnInit {
  public isLoaded = false;
  public isMobile = false;
  loginForm: FormGroup;
  submitClick = false;
  submitted = false;
  returnUrl: string;
  accountInfo: string;
  isError: boolean = false;
  errorMsg = '';

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authenticationService: AuthenticationService,
    private cookieService: CookieService,
    titleService: Title) {
    titleService.setTitle("O10 Demo Portal - Login");
  }

  ngOnInit() {
    var ua = navigator.userAgent;
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua)) {
      this.isMobile = true;
    }
    else {
      this.isMobile = false;
    }

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

    this.http.get<AccountDto>(this.baseUrl + 'Accounts/' + this.route.snapshot.paramMap.get('id')).subscribe(
      r => {
        this.accountInfo = r.accountInfo;
        this.isLoaded = true;
        this.loginForm = this.formBuilder.group({
          password: ['', Validators.required]
        });
      },
      e => {
        console.log(e);
        this.onCancel();
      }
    );
  }

  // convenience getter for easy access to form fields
  get formData() { return this.loginForm.controls; }

  onCancel() {
    this.router.navigate([this.returnUrl]);
  }

  onLogin() {
    this.submitted = true;

    // stop here if form is invalid
    if (this.loginForm.invalid) {
      return;
    }

    this.submitClick = true;
    this.processLogin();
  }

  processLogin() {
    this.authenticationService.login(Number(this.route.snapshot.paramMap.get('id')), this.formData.password.value)
      .pipe(first())
      .subscribe(data => {
        this.cookieService.set("accountId", data.accountId.toString());
        this.navigate(data.accountType);
      }, err => {
          this.processError(err);
          this.submitClick = false;
      });
    }

  navigate(accountType: number) {
    switch (accountType) {
        case 1: {
            this.router.navigate(['/identityProvider']);
            break;
        }
        case 2: {
            this.router.navigate(['/serviceProvider']);
            break;
        }
        case 3: {
            this.router.navigate(['/user']);
            break;
        }
    }
  }

  private processError(err: any) {
    this.isError = true;
    if (err && err.error && err.error.message) {
      this.errorMsg = err.error.message;
    } else if (err && err.error) {
      this.errorMsg = err.error;
    } else {
      this.errorMsg = err;
    }
  }
}

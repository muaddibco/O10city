import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { O10IdentityService } from "../../services/o10identity.service";

@Component({
  selector: 'app-issuer-register',
  templateUrl: './issuer-register.component.html',
  styleUrls: ['./issuer-register.component.scss']
})
export class IssuerRegisterComponent implements OnInit {

  registerForm: FormGroup;
  submitClick = false;
  submitted = false;
  error = '';

  constructor(
    private service: O10IdentityService,
    private formBuilder: FormBuilder,
    private router: Router, 
    titleService: Title) { 
      titleService.setTitle("O10 Demo Portal - Register Issuer");
    }

  ngOnInit(): void {
    this.registerForm = this.formBuilder.group({
      issuerAlias: ['', Validators.required]
    });
  }

  get formData() { return this.registerForm.controls; }

  onRegister() {
    this.submitted = true;
    if (this.registerForm.invalid) {
      return;
    }

    this.submitClick = true;

    this.service.registerIssuer(this.formData.issuerAlias.value)
      .then(
      data => {
        this.router.navigate(['/issuers']);
        },
      error => {
        this.error = error;
        this.submitClick = false;
      });
  }

  onCancel() {
    this.router.navigate(['/issuers']);
  }

}

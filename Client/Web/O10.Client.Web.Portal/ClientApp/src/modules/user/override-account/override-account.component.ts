import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { UserService } from '../user.Service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Title } from '@angular/platform-browser';

@Component({
  templateUrl: './override-account.component.html'
})

export class OverrideAccountComponent implements OnInit {
  public overrideForm: FormGroup;
  submitted = false;
  submitClick = false;
  private secretSpendKey: string;
  private secretViewKey: string;
  private lastCombinedBlockHeight: number;
  private accountId: number;

  constructor(private route: ActivatedRoute, private router: Router, private userService: UserService, private formBuilder: FormBuilder, titleService: Title) {
    let tokenInfo = JSON.parse(sessionStorage.getItem('TokenInfo'));
    this.accountId = tokenInfo.accountId;
    titleService.setTitle("Overriding " + tokenInfo.accountInfo);
  }

  ngOnInit() {
    this.overrideForm = this.formBuilder.group({
      password: ['', Validators.required]
    });
    let actionInfo : string = this.route.snapshot.queryParams['actionInfo'];
    let actionParts: string[] = atob(actionInfo).split(":");
    this.secretSpendKey = actionParts[0];
    this.secretViewKey = actionParts[1];
    this.lastCombinedBlockHeight = Number(actionParts[2]);
  }

  get formData() { return this.overrideForm.controls; }

  onSubmitOverriding() {
    this.submitted = true;

    // stop here if form is invalid
    if (this.overrideForm.invalid) {
      return;
    }

    this.submitClick = true;

    this.userService.overrideUserAccount(this.accountId, this.formData.password.value, this.secretSpendKey, this.secretViewKey, this.lastCombinedBlockHeight).subscribe(r => {
      this.router.navigate(['/accounts']);
    });
  }

  onCancel() {
    this.router.navigate(['/user']);
  }
}

import { Component, OnInit } from '@angular/core';
import { AttributeDefinition, IssuerDetails, O10IdentityService } from "../../services/o10identity.service";
import { Router } from '@angular/router';

@Component({
  selector: 'app-issuer-details-list',
  templateUrl: './issuer-details-list.component.html',
  styleUrls: ['./issuer-details-list.component.scss']
})
export class IssuerDetailsListComponent implements OnInit {

  public issuers: IssuerDetails[];

  constructor(
    private service: O10IdentityService,
    private router: Router) { }

  ngOnInit(): void {
    this.service.getIssuers().then(
      v => {
        console.log(v);
        this.issuers = v;
      }, 
      e => {
        console.error(e);
      })
  }

  onRegister() {
    this.router.navigate(['/registerIssuer']);
  }

  onSetScheme() {
    var defs: AttributeDefinition[] = [
      {
        IsRoot: true,
        AttributeName: "CertificateNumber",
        Alias: "Certificate Number",
        AttributeScheme: "DrivingLicense"
      }
    ];
    this.service.setScheme(defs).then(
      r => {
        console.info(r);
      },
      e => {
        console.error(e);
      }
    )
  }

}

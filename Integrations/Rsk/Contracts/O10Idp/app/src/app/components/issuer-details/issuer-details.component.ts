import { Component, Input, OnInit } from '@angular/core';
import { IssuerDetails } from "../../services/o10identity.service";

@Component({
  selector: 'app-issuer-details',
  templateUrl: './issuer-details.component.html',
  styleUrls: ['./issuer-details.component.scss']
})
export class IssuerDetailsComponent implements OnInit {
  @Input() public issuer: IssuerDetails;

  constructor() { }

  ngOnInit(): void {
  }

}

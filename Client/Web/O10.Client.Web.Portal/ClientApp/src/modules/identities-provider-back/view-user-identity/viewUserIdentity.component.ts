import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { IdentityDto, IdentityAttributesSchemaDto, IdentitiesService } from '../identities.service';
import { Title } from '@angular/platform-browser';

@Component({
  templateUrl: './viewUserIdentity.component.html',
})

export class ViewUserIdentityComponent implements OnInit {

  public identityAttributesSchema: IdentityAttributesSchemaDto;
  public identity: IdentityDto;
  public isLoaded: boolean;
  private accountId: number;

  constructor(private identityService: IdentitiesService, private router: Router, private route: ActivatedRoute, titleService: Title) {
    this.isLoaded = false;
    this.identityAttributesSchema = null;
    let tokenInfo = JSON.parse(sessionStorage.getItem('TokenInfo'));
    this.accountId = tokenInfo.accountId;
    titleService.setTitle(tokenInfo.accountInfo);
  }

  ngOnInit() {
    this.identityService.getIdentityAttributesSchema(this.accountId).subscribe(r => {
      this.identityAttributesSchema = r;
    });

    this.identityService.getIdentity(this.route.snapshot.paramMap.get('id')).subscribe(r => {
      this.identity = r;
      this.isLoaded = true;
    });
  }

  getAssociatedAttrContent(attrName: string) {
    for (var attr of this.identity.attributes) {
      if (attr.attributeName == attrName) {
        return attr.content;
      }
    }

    return null;
  }

  getAssociatedAttrCommitment(attrName: string) {
    for (var attr of this.identity.attributes) {
      if (attr.attributeName == attrName) {
        return attr.originatingCommitment;
      }
    }

    return null;
  }

  onBack() {
    this.router.navigate(['/identityProvider']);
  }
}

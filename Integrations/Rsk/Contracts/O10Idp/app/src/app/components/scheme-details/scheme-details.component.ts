import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { O10IdentityService } from 'src/app/services/o10identity.service';

@Component({
  selector: 'app-scheme-details',
  templateUrl: './scheme-details.component.html',
  styleUrls: ['./scheme-details.component.scss']
})
export class SchemeDetailsComponent implements OnInit {

  public addr: string;

  constructor(
    private service: O10IdentityService,
    private route: ActivatedRoute
  ) { 
    this.addr = this.route.snapshot.queryParamMap.get("address");
  }

  ngOnInit(): void {
    this.service.getScheme(this.addr).then(
      r => {
        console.info(r);
      },
      e => {
        console.error(e);
      }
    );
  }

}

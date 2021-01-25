import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IssuerDetailsListComponent } from './issuer-details-list.component';

describe('IssuerDetailsListComponent', () => {
  let component: IssuerDetailsListComponent;
  let fixture: ComponentFixture<IssuerDetailsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ IssuerDetailsListComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(IssuerDetailsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

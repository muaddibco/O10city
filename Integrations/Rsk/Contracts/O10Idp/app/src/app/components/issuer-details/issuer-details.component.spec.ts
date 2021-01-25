import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IssuerDetailsComponent } from './issuer-details.component';

describe('IssuerDetailsComponent', () => {
  let component: IssuerDetailsComponent;
  let fixture: ComponentFixture<IssuerDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ IssuerDetailsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(IssuerDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

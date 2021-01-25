import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SchemeDetailsComponent } from './scheme-details.component';

describe('SchemeDetailsComponent', () => {
  let component: SchemeDetailsComponent;
  let fixture: ComponentFixture<SchemeDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SchemeDetailsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SchemeDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

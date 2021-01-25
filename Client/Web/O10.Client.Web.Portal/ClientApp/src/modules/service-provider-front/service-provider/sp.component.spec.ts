import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SpComponent } from './sp.component';

describe('SpComponent', () => {
  let component: SpComponent;
  let fixture: ComponentFixture<SpComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [SpComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('initializeSession', async(() => {
    component.initializeSession();
    component.pageTitle = "qqq"
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('qqq');
  }));

  it('onReset', async(() => {
    component.onReset();
    component.pageTitle = "bbb"

    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('bbb');
  }));
});

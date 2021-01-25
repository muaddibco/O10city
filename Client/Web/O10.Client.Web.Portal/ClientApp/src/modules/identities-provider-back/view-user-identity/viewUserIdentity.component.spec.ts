import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewUserIdentityComponent } from './viewUserIdentity.component';

describe('ViewUserIdentityComponent', () => {
  let component: ViewUserIdentityComponent;
  let fixture: ComponentFixture<ViewUserIdentityComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ViewUserIdentityComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewUserIdentityComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('ngOnInit', async(() => {
    component.ngOnInit()
    const titleText = fixture.nativeElement.querySelector('h4').textContent;
    expect(titleText).toEqual('Root Identity');
  }));

  it('getAssociatedAttrCommitment', async(() => {
    var val = component.getAssociatedAttrCommitment("111");
    expect(val).toEqual('222');
  }));

  it('getAssociatedAttrCommitment Neg', async(() => {
    var val = component.getAssociatedAttrCommitment(null);
    expect(val).toEqual(null);
  }));

  it('getAssociatedAttrContent', async(() => {
    var val = component.getAssociatedAttrContent("111");
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(val).toEqual('attr');
  }));

  it('getAssociatedAttrContent Neg', async(() => {
    var val = component.getAssociatedAttrContent(null);
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(val).toEqual(null);
  }));

  it('onBack', async(() => {
    component.onBack();
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('onSend', async(() => {
    component.onSend();
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));
});

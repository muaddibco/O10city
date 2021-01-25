import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UserService } from './user.Service';
import { assertNotNull } from '@angular/compiler/src/output/output_ast';

describe('UserService', () => {
  let component: UserService;
  let fixture: ComponentFixture<UserService>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [UserService]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserService);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('getUserDetails', async(() => {
    var userObservable = component.getUserDetails();
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(userObservable).toBeDefined();
  }));

  it('duplicateUser', async(() => {
    try {
      component.duplicateUser("A", "B");
    } catch (e) {
      fail();
    }
  }));

  it('getActionType', async(() => {
    var obs = component.getActionType("FF");
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(obs).toBeDefined();
  }));


  it('getUserAssociatedAttributes', async(() => {
    component.getUserAssociatedAttributes("");
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('getUserAttributes', async(() => {
    component.getUserAttributes();
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('requestIdentity1', async(() => {
    component.requestIdentity("p", "idCard", "pwd", null);
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('requestIdentity2', async(() => {
    component.requestIdentity("p", "idCard", "pwd", "xcvfdsa");
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('sendCompromisedProofs Neg', async(() => {
    component.sendCompromisedProofs(null);
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('updateUserAssociatedAttributes Neg', async(() => {
    component.updateUserAssociatedAttributes(null);
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));
});


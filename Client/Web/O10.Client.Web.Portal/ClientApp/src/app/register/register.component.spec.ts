import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [RegisterComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('onRegister', async(() => {
    component.onRegister();
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('Register a new account');
  }));

  it('selectIP', async(() => {
    component.selectIP();
    var btn = fixture.nativeElement.querySelector('btn');
    expect(btn).toBeDefined();
  }));

  it('selectSP', async(() => {
    component.selectSP();
    var btn = fixture.nativeElement.querySelector('btn');
    expect(btn).toBeDefined();
  }));

  it('selectUser', async(() => {
    component.selectUser();
    var btn = fixture.nativeElement.querySelector('btn');
    expect(btn).toBeDefined();
  }));

  it('onCancel', async(() => {
    component.onCancel();
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('Register a new account');
  }));
});

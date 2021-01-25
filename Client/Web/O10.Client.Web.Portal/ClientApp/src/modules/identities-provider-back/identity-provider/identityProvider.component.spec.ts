import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IdentityProviderComponent } from './identityProvider.component';

describe('IdentityProviderComponent', () => {
  let component: IdentityProviderComponent;
  let fixture: ComponentFixture<IdentityProviderComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [IdentityProviderComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IdentityProviderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should display a title', async(() => {
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));
});

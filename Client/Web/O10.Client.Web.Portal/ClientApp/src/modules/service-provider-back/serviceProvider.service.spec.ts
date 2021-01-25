import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ServiceProviderService } from './serviceProvider.service';

describe('UserService', () => {
  let component: ServiceProviderService;
  let fixture: ComponentFixture<ServiceProviderService>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ServiceProviderService]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ServiceProviderService);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('getIdentityAttributeValidationDescriptors', async(() => {
    component.getIdentityAttributeValidationDescriptors();
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('getIdentityAttributeValidations', async(() => {
    component.getIdentityAttributeValidations();
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('getRegistrations', async(() => {
    component.getRegistrations();
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('getServiceProvider', async(() => {
    component.getServiceProvider("aaaa");
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('getServiceProvider Neg', async(() => {
    component.getServiceProvider(null);
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('saveIdentityAttributeValidations Neg', async(() => {
    component.saveIdentityAttributeValidations(null);
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));
});


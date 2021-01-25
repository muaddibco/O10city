import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ServiceProviderLoginRegComponent } from './service-provider.component';

describe('ServiceProviderLoginRegComponent', () => {
  let component: ServiceProviderLoginRegComponent;
  let fixture: ComponentFixture<ServiceProviderLoginRegComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ServiceProviderLoginRegComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ServiceProviderLoginRegComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('cameraWasSwitched', async(() => {
    component.cameraWasSwitched("11");
    var camera = fixture.nativeElement.querySelector('webcam');
    expect(camera).toBeDefined();
  }));

  it('cameraWasSwitched Neg', async(() => {
    component.cameraWasSwitched(null);
    const titleText = fixture.nativeElement.querySelector('h1').textContent;
    expect(titleText).toEqual('MIA');
  }));

  it('onCancel', async(() => {
    component.onCancel();
    const titleText = fixture.nativeElement.querySelector('label').textContent;
    expect(titleText).toEqual('Please select identity attribute for action:');
  }));

  it('onChange neg', async(() => {
    component.onChange(null);
    const titleText = fixture.nativeElement.querySelector('label').textContent;
    expect(titleText).toEqual('Please select identity attribute for action:');
  }));

  it('onToggleCamera once', async(() => {
    component.onToggleCamera();
    var camera = fixture.nativeElement.querySelector('webcam');
    expect(camera).toBeDefined();
  }));

  it('onToggleCamera twice', async(() => {
    component.onToggleCamera();
    component.onToggleCamera();
    var camera = fixture.nativeElement.querySelector('webcam');
    expect(camera).toBeDefined();
  }));

  it('onToggleCamera three', async(() => {
    component.onToggleCamera();
    component.onToggleCamera();
    component.onToggleCamera();
    var camera = fixture.nativeElement.querySelector('webcam');
    expect(camera).toBeDefined();
  }));
});

import { TestBed } from '@angular/core/testing';

import { O10IdentityService } from './o10identity.service';

describe('O10identityService', () => {
  let service: O10IdentityService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(O10IdentityService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

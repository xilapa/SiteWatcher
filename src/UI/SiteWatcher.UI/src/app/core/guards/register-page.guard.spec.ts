import { TestBed } from '@angular/core/testing';

import { RegisterPageGuard } from './register-page.guard';

describe('RegisterPageGuard', () => {
  let guard: RegisterPageGuard;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    guard = TestBed.inject(RegisterPageGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });
});

import { TestBed } from '@angular/core/testing';

import { LoginPageGuard } from './login-page.guard';

describe('LoginPageGuard', () => {
  let guard: LoginPageGuard;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    guard = TestBed.inject(LoginPageGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });
});

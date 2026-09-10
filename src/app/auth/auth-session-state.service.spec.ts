import { TestBed } from '@angular/core/testing';
import { AuthSessionStateService } from './auth-session-state.service';

describe('AuthSessionStateService', (): void => {
  let service: AuthSessionStateService;

  beforeEach((): void => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthSessionStateService);
  });

  it('allows content administrators to manage content', (): void => {
    service.setAuthenticated([
      { type: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role', value: 'contentadmin' }
    ]);

    expect(service.canManageContent()).toBeTrue();
    expect(service.isAdmin()).toBeFalse();
  });

  it('allows administrators to manage content', (): void => {
    service.setAuthenticated([
      { type: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role', value: 'admin' }
    ]);

    expect(service.canManageContent()).toBeTrue();
    expect(service.isAdmin()).toBeTrue();
  });

  it('denies unauthenticated users', (): void => {
    service.setUnauthenticated();

    expect(service.canManageContent()).toBeFalse();
  });
});

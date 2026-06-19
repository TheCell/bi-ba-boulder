import { Injectable, inject } from '@angular/core';
import { Subject } from 'rxjs';
import { BffAuthService } from './bff-auth.service';
import { HttpContext } from '@angular/common/http';
import { SKIP_ERROR_HANDLER } from '../core/interceptors/error-handler-interceptor';
import { DevAuthService } from '@api-net/index';
import { AuthSessionStateService } from './auth-session-state.service';

@Injectable({
  providedIn: 'root'
})
export class LoginTrackerService {
  private bffAuthService = inject(BffAuthService);
  private devAuthService = inject(DevAuthService);
  private authSessionStateService = inject(AuthSessionStateService);

  /** Emits whenever the authentication state changes. */
  public authStateChanged$: Subject<boolean> = this.authSessionStateService.authStateChanged$;

  /**
   * Fetches the current session state from the BFF.
   * Call this on app startup to initialize the login state.
   */
  public checkSession(): void {
    this.bffAuthService.getUser(new HttpContext().set(SKIP_ERROR_HANDLER, true)).subscribe({
      next: (claims) => {
        this.authSessionStateService.setAuthenticated(claims);
      },
      error: () => {
        this.authSessionStateService.setUnauthenticated();
      }
    });
  }

  public isLoggedIn(): boolean {
    return this.authSessionStateService.isLoggedIn();
  }

  public getUserMail(): string | undefined {
    return this.getClaimValue('email');
  }

  public getUserName(): string | undefined {
    return this.getClaimValue('name') ?? this.getClaimValue('preferred_username') ?? this.getUserMail();
  }

  /**
   * Initiates the OIDC login flow via browser navigation.
   * @param returnUrl URL to redirect back to after successful login. Defaults to current path.
   */
  public login(returnUrl?: string): void {
    const url = this.bffAuthService.getLoginUrl(returnUrl ?? window.location.pathname);
    window.location.href = url;
  }

  /**
   * Development-only login that bypasses OIDC.
   * @param email Email of the test user. Defaults to "dev@test.local".
   */
  public devLogin(email = 'dev@test.local'): void {
    this.devAuthService.devLogin(email).subscribe({
      next: () => {
        this.checkSession();
      },
      error: (err) => {
        console.error('Dev login failed:', err);
      }
    });
  }

  /**
   * Signs out of the BFF session and OIDC provider.
   */
  public logout(): void {
    this.bffAuthService.logout().subscribe({
      next: () => {
        this.authSessionStateService.setUnauthenticated();
        window.location.href = '/';
      },
      error: () => {
        this.authSessionStateService.setUnauthenticated();
      }
    });
  }

  private getClaimValue(type: string): string | undefined {
    return this.authSessionStateService.getClaimValue(type);
  }
}

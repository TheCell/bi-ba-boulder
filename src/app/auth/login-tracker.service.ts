import { Injectable, inject } from '@angular/core';
import { Subject } from 'rxjs';
import { BffAuthService, BffUserClaim } from './bff-auth.service';

@Injectable({
  providedIn: 'root',
})
export class LoginTrackerService {
  private bffAuthService = inject(BffAuthService);
  private claims: BffUserClaim[] = [];
  private authenticated = false;

  /** Emits whenever the authentication state changes. */
  public authStateChanged$ = new Subject<boolean>();

  /**
   * Fetches the current session state from the BFF.
   * Call this on app startup to initialize the login state.
   */
  public checkSession(): void {
    this.bffAuthService.getUser().subscribe({
      next: (claims) => {
        this.claims = claims;
        this.authenticated = true;
        this.authStateChanged$.next(true);
      },
      error: () => {
        this.claims = [];
        this.authenticated = false;
        this.authStateChanged$.next(false);
      },
    });
  }

  public isLoggedIn(): boolean {
    return this.authenticated;
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
   * Signs out of the BFF session and OIDC provider.
   */
  public logout(): void {
    this.bffAuthService.logout().subscribe({
      next: () => {
        this.claims = [];
        this.authenticated = false;
        this.authStateChanged$.next(false);
        window.location.href = '/';
      },
      error: () => {
        // Even on error, clear local state
        this.claims = [];
        this.authenticated = false;
        this.authStateChanged$.next(false);
      },
    });
  }

  private getClaimValue(type: string): string | undefined {
    const claim = this.claims.find((c) => c.type === type);
    return claim?.value;
  }
}

import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { BffUserClaim } from './bff-auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthSessionStateService {
  private claims: BffUserClaim[] = [];
  private authenticated = false;

  public authStateChanged$ = new Subject<boolean>();

  public setAuthenticated(claims: BffUserClaim[]): void {
    this.claims = claims;
    this.setAuthenticatedFlag(true);
  }

  public setUnauthenticated(): void {
    this.claims = [];
    this.setAuthenticatedFlag(false);
  }

  public isLoggedIn(): boolean {
    return this.authenticated;
  }

  /**
   * Returns true if the user has the "admin" role claim. This will be reworked in the future
   * @returns
   */
  public isAdmin(): boolean {
    if (!this.authenticated) {
      return false;
    }

    return this.claims.some((claim) => claim.type.indexOf('claims/role') !== -1 && claim.value === 'admin');
  }

  public getClaimValue(type: string): string | undefined {
    const claim = this.claims.find((item) => item.type === type);
    return claim?.value;
  }

  private setAuthenticatedFlag(isAuthenticated: boolean): void {
    if (this.authenticated === isAuthenticated) {
      return;
    }

    this.authenticated = isAuthenticated;
    this.authStateChanged$.next(isAuthenticated);
  }
}

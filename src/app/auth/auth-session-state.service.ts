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

  public isAdmin(): boolean {
    return this.hasRole('admin');
  }

  public canManageContent(): boolean {
    return this.hasRole('contentadmin') || this.hasRole('admin');
  }

  public hasRole(role: string): boolean {
    return (
      this.authenticated && this.claims.some((claim) => claim.type.includes('claims/role') && claim.value === role)
    );
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

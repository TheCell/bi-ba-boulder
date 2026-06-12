import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { BffUserClaim } from './bff-auth.service';

@Injectable({
  providedIn: 'root',
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
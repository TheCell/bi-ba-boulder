import { Injectable } from '@angular/core';
import { TokenDto } from '@api/index';
import { jwtDecoder } from './jwt-decoder';

@Injectable({
  providedIn: 'root'
})
export class LoginTrackerService {
  private token?: string;
  private expiration?: Date;

  public constructor() {
    const token = localStorage.getItem('auth_token');
    const expirationString = localStorage.getItem('auth_token_expiry');
    if (token && expirationString) {
      this.token = token;
      const expirationValueOf = parseInt(expirationString);
      this.expiration = new Date(expirationValueOf);
    }
  }

  public isLoggedIn(): boolean {
    if (!this.expiration || !this.token) {
      return false;
    }

    const now = new Date();
    return this.expiration.valueOf() > now.valueOf();
  }

  public getUserMail(): string | undefined {
    if (this.token) {
      return jwtDecoder(this.token).email;
    }

    return undefined;
  }

  public saveLoginInformation(token: TokenDto): void {
    const decodedToken = jwtDecoder(token.token);
    localStorage.setItem('auth_token', token.token);
    localStorage.setItem('auth_token_expiry', '' + decodedToken.exp * 1000);
    this.token = token.token;
    this.expiration = new Date(decodedToken.exp * 1000);
  }

  public removeLoginInformation(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_token_expiry');
    this.token = undefined;
    this.expiration = undefined;
  }

  public getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

}

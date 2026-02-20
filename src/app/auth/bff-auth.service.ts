import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface BffUserClaim {
  type: string;
  value: string;
}

@Injectable({
  providedIn: 'root',
})
export class BffAuthService {
  private http = inject(HttpClient);

  /**
   * Returns the current user's claims from the BFF session.
   * Returns 401 if the user is not authenticated.
   */
  public getUser(): Observable<BffUserClaim[]> {
    return this.http.get<BffUserClaim[]>(`${environment.apiURL}/bff/user`);
  }

  /**
   * Signs out of the local cookie session and the OIDC Identity Provider.
   */
  public logout(): Observable<void> {
    return this.http.post<void>(`${environment.apiURL}/bff/logout`, null);
  }

  /**
   * Returns the URL to navigate to for OIDC login.
   * This is a browser navigation (not an AJAX call) -- the backend will redirect to the IdP.
   * @param returnUrl URL to redirect back to after successful login. Defaults to "/".
   */
  public getLoginUrl(returnUrl = '/'): string {
    return `${environment.apiURL}/bff/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  }
}

import { HttpEvent, HttpHandlerFn, HttpRequest, type HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { LoginTrackerService } from 'src/app/auth/login-tracker.service';
import { environment } from 'src/environments/environment';

export const loggedInInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const loginTrackerService = inject(LoginTrackerService);
  
  if (req.url.startsWith(environment.apiURL)) {
    
    const expiration = Number.parseInt(localStorage.getItem('auth_token_expiry') ?? '');
    if (isNaN(expiration) || expiration < Date.now()) {
      loginTrackerService.removeLoginInformation();
      return next(req);
    }

    const token = loginTrackerService.getToken();
    if (token) {
      const newReq = req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) });
      return next(newReq);
    }
  }
  
  return next(req);
};

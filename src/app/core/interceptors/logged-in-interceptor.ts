import { HttpEvent, HttpHandlerFn, HttpRequest, type HttpInterceptorFn } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export const loggedInInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  if (req.url.startsWith(environment.apiURL)) {
    const token = localStorage.getItem('auth_token');
    const expiration = Number.parseInt(localStorage.getItem('auth_token_expiry') ?? '');
    if (isNaN(expiration) || expiration < Date.now()) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('auth_token_expiry');
      return next(req);
    }

    if (token) {
      const newReq = req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) });
      return next(newReq);
    }
  }
  
  return next(req);
};

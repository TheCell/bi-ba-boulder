import { HttpEvent, HttpHandlerFn, HttpRequest, type HttpInterceptorFn } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

/**
 * Adds the X-CSRF header to all AJAX requests targeting the backend API.
 *
 * In the BFF pattern, authentication is cookie-based. The X-CSRF header protects
 * against CSRF attacks because browsers won't add custom headers to cross-origin
 * "simple" requests, and adding a custom header triggers a CORS preflight check.
 *
 * The backend requires this header on all /api/* requests.
 */
export const csrfInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  if (req.url.startsWith(environment.apiURL)) {
    const csrfReq = req.clone({
      headers: req.headers.set('X-CSRF', '1'),
    });
    return next(csrfReq);
  }

  return next(req);
};

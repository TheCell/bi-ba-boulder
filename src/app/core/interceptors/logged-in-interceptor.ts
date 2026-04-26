import { HttpEvent, HttpHandlerFn, HttpRequest, type HttpInterceptorFn } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * In the BFF pattern, authentication is handled via HttpOnly cookies that the browser
 * sends automatically. This interceptor is retained as a no-op placeholder in case
 * request-level auth logic is needed in the future.
 *
 * The CSRF header is handled by the csrfInterceptor.
 * Cookies are sent automatically when withCredentials is true (configured in app.config.ts).
 */
export const loggedInInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  return next(req);
};

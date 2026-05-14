import { HttpContextToken, HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest, type HttpInterceptorFn } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ToastService } from '../toast-container/toast.service';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';

export const SKIP_ERROR_HANDLER = new HttpContextToken<boolean>(() => false);

export const errorHandlerInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const toastService = inject(ToastService);

  if (req.context.get(SKIP_ERROR_HANDLER)) {
    return next(req);
  }

  return next(req).pipe((source) => {
    return new Observable<HttpEvent<unknown>>((observer) => {
      return source.subscribe({
        next: (event) => {
          observer.next(event);
        },
        error: (err: HttpErrorResponse) => {
          const title = `Error: ${err.status} (${err.statusText})`;
          let message = '';
          
          if (environment.production) {
            if (err.status === 401) {
              message = 'You are not logged in or your session has expired.';
            } else if (err.status === 403) {
              message = 'You do not have permission to perform this action.';
            } else if (err.status === 500) {
              message = 'A server error occurred. Please try again later.';
            } else if (err.error) {
              const error = err.error;
              if (typeof error.error === 'string') {
                message = message.concat(`${error.error}`);
              } else if (error && typeof error.message === 'string') {
                message = message.concat(`${error.message}`);
              } else {
                message = 'An error occurred while processing your request.';
              }
            } else {
              message = 'An error occurred while processing your request.';
            }
          } else {
            message = err.error.message;
          }

          toastService.showDanger(title, message);
          observer.error(err);
        },
        complete: () => {
          observer.complete();
        }
      });
    });
  });
};

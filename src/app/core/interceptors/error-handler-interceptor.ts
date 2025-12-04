import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest, type HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ToastService } from '../toast-container/toast.service';

export const errorHandlerInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const toastService = inject(ToastService);

  return next(req).pipe((source) => {
    return new Observable<HttpEvent<unknown>>((observer) => {
      return source.subscribe({
        next: (event) => {
          observer.next(event);
        },
        error: (err: HttpErrorResponse) => {
          console.error('HTTP Error occurred:', err);
          const title = `Error: ${err.status} (${err.statusText})`;
          let message = '';
          if (err.error) {
            if (typeof err.error.error === 'string') {
              message = message.concat(`${err.error.error}`);
            } else if (err.error && typeof err.error.message === 'string') {
              message = message.concat(`${err.error.message}`);
            } else {
              message = 'An error occurred while processing your request.';
            }
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

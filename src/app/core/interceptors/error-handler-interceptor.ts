import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest, type HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ToastService } from '../toast-container/toast.service';
import { LoginTrackerService } from 'src/app/auth/login-tracker.service';

export const errorHandlerInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  const toastService = inject(ToastService);
  const loginTrackerService = inject(LoginTrackerService);

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
            const error = err.error;
            if (typeof error.error === 'string') {
              message = message.concat(`${error.error}`);
            } else if (error && typeof error.message === 'string') {
              message = message.concat(`${error.message}`);
            } else {
              message = 'An error occurred while processing your request.';
            }

            if (error.code === 401)
            {
              if (error.message.localeCompare('Expired JWT Token') === 0) {
                message = 'Logged you out.';
                loginTrackerService.removeLoginInformation();
              }
              else if (error.message.localeCompare('JWT Token not found') === 0) {
                message = 'You are not logged in.';
              }

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

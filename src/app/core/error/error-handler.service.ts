import { Injectable, ErrorHandler } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService implements ErrorHandler {

  constructor() {
    // super();
  }

  public handleError(error: unknown): void {
    
    // todo add toasty
    console.error('An error occurred:', error);
  }

}

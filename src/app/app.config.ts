import { ApplicationConfig, ErrorHandler, Provider, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { ApiModule, Configuration, ConfigurationParameters } from './api';
import { environment } from '../environments/environment';
import { ErrorHandlerService } from './core/error/error-handler.service';

function apiConfigFactory(): Configuration {
  const params: ConfigurationParameters = {
    basePath: environment.apiURL,
    withCredentials: false
  }
  return new Configuration(params);
}

const configurationProvider: Provider = {
  provide: Configuration,
  useFactory: apiConfigFactory,
  // (new Configuration()),
};

export const appConfig: ApplicationConfig = {
  providers: [
    configurationProvider,
    provideZoneChangeDetection({ eventCoalescing: true }),
    // provideRouter(routes, withDebugTracing()),
    provideRouter(routes),
    provideHttpClient(
      withFetch()
    ),
    { provide: ErrorHandler, useClass: ErrorHandlerService }
  ]
};

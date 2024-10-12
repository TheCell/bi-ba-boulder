import { ApplicationConfig, Provider, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { Configuration, ConfigurationParameters } from './api';
import { environment } from '../environments/environment';

function apiConfigFactory (): Configuration {
  const params: ConfigurationParameters = {
    basePath: environment.apiURL,
    withCredentials: false
  }
  return new Configuration(params);
}

const configurationProvider: Provider = {
  provide: Configuration,
  useFactory: apiConfigFactory,
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
  ]
};

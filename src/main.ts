import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import './app/extensions/string.extensions';

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));

  // todo: add https://www.angular-auth-oidc-client.com/docs/intro
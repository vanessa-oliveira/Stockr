import { ApplicationConfig, provideZoneChangeDetection, LOCALE_ID } from '@angular/core';
import { provideRouter } from '@angular/router';
import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';

import { routes } from './app.routes';
import {providePrimeNG} from 'primeng/config';
import Material from '@primeuix/themes/aura';
import {provideHttpClient} from '@angular/common/http';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {provideEnvironmentNgxMask} from 'ngx-mask';

registerLocaleData(localePt);

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    provideEnvironmentNgxMask(),
    provideHttpClient(),
    { provide: LOCALE_ID, useValue: 'pt' },
    providePrimeNG({
      theme: {
        preset: Material,
        options: {
          darkModeSelector: false,
        }
      }
    })
  ]
};

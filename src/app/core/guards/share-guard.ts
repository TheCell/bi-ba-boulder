import { inject } from '@angular/core';
import { Router } from '@angular/router';
import type { CanActivateFn } from '@angular/router';
import { MediasService, UriAliasDto } from '@api-net/index';
import { UriType } from '../enums/uri-type.enum';
import { map } from 'rxjs';

export const shareGymGuard: CanActivateFn = (route) => {
  const mediasService = inject(MediasService);
  const router = inject(Router);
  const alias = route.params['alias'];

  return mediasService.getUriAlias(alias, UriType.BoulderGym).pipe(
    map((uriAlias: UriAliasDto | null) => {
      if (uriAlias?.id) {
        return router.createUrlTree(['/boulder-gym', uriAlias.id]);
      }
      return router.createUrlTree(['/not-found']);
    })
  );
};

export const shareOutdoorGuard: CanActivateFn = (route) => {
  const mediasService = inject(MediasService);
  const router = inject(Router);
  const alias = route.params['alias'];
  console.log(alias);

  return mediasService.getUriAlias(alias, UriType.OutdoorArea).pipe(
    map((uriAlias: UriAliasDto | null) => {
      if (uriAlias?.id) {
        return router.createUrlTree(['/outdoor-area', uriAlias.id]);
      }
      return router.createUrlTree(['/not-found']);
    })
  );
};


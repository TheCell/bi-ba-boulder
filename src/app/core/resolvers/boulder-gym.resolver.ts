import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { BoulderGymDto, BoulderGymService } from '@api-net/index';

export const boulderGymResolver: ResolveFn<BoulderGymDto> = (
  route: ActivatedRouteSnapshot,
  _state: RouterStateSnapshot
) => {
  const boulderGymId = route.paramMap.get('boulderGymId');

  if (!boulderGymId) {
    throw new Error('Boulder Gym ID is missing in route parameters');
  }

  return inject(BoulderGymService).getBoulderGym(boulderGymId);
};

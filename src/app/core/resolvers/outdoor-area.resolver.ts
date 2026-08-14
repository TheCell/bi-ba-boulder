import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { OutdoorAreaDto, OutdoorAreasService } from '@api-net/index';

export const outdoorAreaResolver: ResolveFn<OutdoorAreaDto> = (
  route: ActivatedRouteSnapshot,
  _state: RouterStateSnapshot
) => {
  const outdoorAreaId = route.paramMap.get('outdoorAreaId');

  if (!outdoorAreaId) {
    throw new Error('Outdoor Area ID is missing in route parameters');
  }

  return inject(OutdoorAreasService).getOutdoorArea(outdoorAreaId);
};

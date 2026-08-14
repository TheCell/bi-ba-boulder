import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { SectorDto, SectorsService } from '@api-net/index';

export const sectorsResolver: ResolveFn<SectorDto[]> = (_route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) =>
  inject(SectorsService).getSectors();

export const sectorResolver: ResolveFn<SectorDto> = (route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => {
  const sectorId = route.paramMap.get('sectorId');

  if (!sectorId) {
    throw new Error('Sector ID is missing in route parameters');
  }

  return inject(SectorsService).getSector(sectorId);
};

import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { SectorDto, SectorsService } from '@api-net/index';

export const sectorsResolver: ResolveFn<SectorDto[]> = (_route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => inject(SectorsService).getSectors();

import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { SectorDto, SectorService } from '@api/index';

export const sectorsResolver: ResolveFn<SectorDto[]> = (_route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => inject(SectorService).getSectors();

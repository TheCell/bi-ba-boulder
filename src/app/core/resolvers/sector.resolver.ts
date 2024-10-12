import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { DefaultService, SectorDto } from '../../api';

export const sectorsResolver: ResolveFn<SectorDto[]> = (_route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => inject(DefaultService).getSectors();

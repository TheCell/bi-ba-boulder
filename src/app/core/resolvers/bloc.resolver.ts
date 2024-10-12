import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { BlocDto, DefaultService } from '../../api';

export const blocsOfSectorResolver: ResolveFn<BlocDto[]> = (route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => inject(DefaultService).getBlocsBySectorId(route.paramMap.get('sectorId')!);

export const blocResolver: ResolveFn<BlocDto> = (route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => inject(DefaultService).getBloc(route.paramMap.get('id')!);

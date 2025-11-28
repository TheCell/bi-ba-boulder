import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { BlocDto, BlocService } from '@api/index';

export const blocsOfSectorResolver: ResolveFn<BlocDto[]> = (route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => {
    const sectorId = route.paramMap.get('sectorId');
    
    if (!sectorId) {
        throw new Error('Sector ID is missing in route parameters');
    }
    
    return inject(BlocService).getBlocsBySectorId(sectorId)};

export const blocResolver: ResolveFn<BlocDto> = (route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => {
    const id = route.paramMap.get('id');
    
    if (!id) {
        throw new Error('Bloc ID is missing in route parameters');
    }
    
    return inject(BlocService).getBloc(id);
}
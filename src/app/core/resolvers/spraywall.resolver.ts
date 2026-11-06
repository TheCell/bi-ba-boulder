import { inject } from '@angular/core';
import type { ResolveFn } from '@angular/router';
import { SpraywallDto, SpraywallsService } from '@api-net/index';
import { of } from 'rxjs';

export const spraywallResolver: ResolveFn<SpraywallDto | undefined> = (route, _state) => {
  const spraywallsService = inject(SpraywallsService);
  const spraywallId = route.paramMap.get('spraywallId') ?? route.paramMap.get('id');
  if (spraywallId) {
    return spraywallsService.getSpraywall(spraywallId);
  }
  return of(undefined);
};


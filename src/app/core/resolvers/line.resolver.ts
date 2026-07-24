import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { LinesService } from '@api-net/api/api';
import { LineDto } from '@api-net/model/models';

export const lineResolver: ResolveFn<LineDto> = (route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => {
  const lineId = route.paramMap.get('lineId');

  if (!lineId) {
    throw new Error('Line ID is missing in route parameters');
  }

  return inject(LinesService).getLine(lineId);
};


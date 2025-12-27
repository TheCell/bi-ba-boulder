import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { SpraywallProblemDto, SpraywallProblemsService } from '@api/index';
import { Observable, of } from 'rxjs';

export const spraywallProblemResolver: ResolveFn<Observable<SpraywallProblemDto | undefined>> = (route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => {
  const spraywallProblemsService = inject(SpraywallProblemsService);
  const spraywallProblemId = route.paramMap.get('problemId');
  if (spraywallProblemId) {
    return spraywallProblemsService.getProblem(spraywallProblemId);
  }
  return of();
};

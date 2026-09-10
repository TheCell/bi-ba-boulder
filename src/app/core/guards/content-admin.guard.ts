import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthSessionStateService } from '../../auth/auth-session-state.service';

export const contentAdminGuard: CanActivateFn = () => {
  const authSessionStateService = inject(AuthSessionStateService);
  const router = inject(Router);

  return authSessionStateService.canManageContent() || router.createUrlTree(['/home']);
};
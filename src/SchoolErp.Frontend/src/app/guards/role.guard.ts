import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const expectedRoles = route.data?.['roles'] as string[] | undefined;

  if (!authService.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  if (!expectedRoles?.length || authService.hasAnyRole(expectedRoles)) {
    return true;
  }

  if (authService.hasRole('Student')) {
    return router.createUrlTree(['/student']);
  }

  return router.createUrlTree(['/dashboard']);
};

import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    if (localStorage.getItem('mustChangePassword') === 'true' && state.url !== '/change-password') {
      return router.createUrlTree(['/change-password']);
    }
    return true;
  }

  return router.createUrlTree(['/login']);
};

import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Allows access only to users with the 'Student' role.
 * Unauthenticated <span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">arrow_forward</span> /login
 * Authenticated but wrong role <span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">arrow_forward</span> /dashboard
 */
export const studentGuard: CanActivateFn = () => {
  const auth   = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/login']);
  }

  if (auth.hasRole('Student')) {
    return true;
  }

  // Admin / Teacher / Parent hit the main dashboard
  return router.createUrlTree(['/dashboard']);
};

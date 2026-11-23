import { inject } from '@angular/core';

import {
  CanActivateFn,
  Router
} from '@angular/router';

import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route) => {

  const authService = inject(AuthService);
  const router = inject(Router);

  // Not authenticated: redirect to login.
  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  // Retrieve allowed roles from route configuration.
  const allowedRoles = route.data['roles'] as string[];

  // Check whether user has at least one permitted role.
  if (authService.hasAnyRole(allowedRoles)) {
    return true;
  }

  // Authenticated but unauthorized:
  // redirect to the user's own dashboard.
  return router.createUrlTree([
    authService.getHomeRoute()
  ]);
};

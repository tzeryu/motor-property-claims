import { inject } from '@angular/core';

import {
  CanActivateFn,
  Router
} from '@angular/router';

import { AuthService } from '../services/auth.service';

export const guestGuard: CanActivateFn = () => {

  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {

    const homeRoute = authService.getHomeRoute();

    if (homeRoute !== '/login') {
      return router.createUrlTree([homeRoute]);
    }
  }

  return true;
};

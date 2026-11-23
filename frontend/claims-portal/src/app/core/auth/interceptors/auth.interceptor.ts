import { inject } from '@angular/core';

import {
  HttpErrorResponse,
  HttpInterceptorFn
} from '@angular/common/http';

import { catchError, throwError } from 'rxjs';

import { AuthService } from '../services/auth.service';
import { environment } from '../../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (request, next) => {

  const authService = inject(AuthService);

  // Only attach tokens to requests sent to our own API.
  const isApiRequest = request.url.startsWith(
    environment.apiBaseUrl
  );

  const isLoginRequest = request.url.startsWith(
    `${environment.apiBaseUrl}/auth/login`
  );

  if (!isApiRequest || isLoginRequest) {
    return next(request);
  }

  const authorizationHeader =
    authService.getAuthorizationHeader();

  if (!authorizationHeader) {
    return next(request);
  }

  const authenticatedRequest = request.clone({
    setHeaders: {
      Authorization: authorizationHeader
    }
  });

  return next(authenticatedRequest).pipe(
    catchError((error: unknown) => {

      if (
        error instanceof HttpErrorResponse &&
        error.status === 401
      ) {
        authService.logout();
      }

      return throwError(() => error);
    })
  );
};

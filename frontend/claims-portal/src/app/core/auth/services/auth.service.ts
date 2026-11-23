import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

import { BehaviorSubject, Observable, tap } from 'rxjs';

import { environment } from '../../../../environments/environment';

import {
  AuthSession,
  AuthenticatedUser,
  LoginRequest,
  LoginResult
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly storageKey = 'claims-portal-session';

  private session: AuthSession | null = this.restoreSession();

  private readonly currentUserSubject =
    new BehaviorSubject<AuthenticatedUser | null>(
      this.session?.user ?? null
    );

  readonly currentUser$ = this.currentUserSubject.asObservable();

  get currentUser(): AuthenticatedUser | null {
    return this.currentUserSubject.value;
  }

  // Authenticate against the .NET API.
  login(request: LoginRequest): Observable<LoginResult> {

    return this.http.post<LoginResult>(
      `${environment.apiBaseUrl}/auth/login`,
      request
    ).pipe(
      tap(result => {

        if (
          !result.accessToken ||
          !result.tokenType ||
          !result.userId ||
          !result.email ||
          !result.fullName ||
          !Array.isArray(result.roles)
        ) {
          throw new Error('Invalid authentication response.');
        }

        const user: AuthenticatedUser = {
          userId: result.userId,
          email: result.email,
          fullName: result.fullName,
          roles: result.roles
        };

        const session: AuthSession = {
          accessToken: result.accessToken,
          tokenType: result.tokenType,
          expiresAtUtc: result.expiresAtUtc,
          user
        };

        this.session = session;

        sessionStorage.setItem(
          this.storageKey,
          JSON.stringify(session)
        );

        this.currentUserSubject.next(user);
      })
    );
  }

  getAccessToken(): string | null {

    if (!this.session) {
      return null;
    }

    const expiry = Date.parse(this.session.expiresAtUtc);

    if (!Number.isFinite(expiry) || expiry <= Date.now()) {
      this.clearSession();
      return null;
    }

    return this.session.accessToken;
  }

  getAuthorizationHeader(): string | null {

    const token = this.getAccessToken();

    if (!token || !this.session) {
      return null;
    }

    return `${this.session.tokenType} ${token}`;
  }

  isAuthenticated(): boolean {

    return this.getAccessToken() !== null &&
      this.currentUser !== null;
  }

  hasRole(role: string): boolean {

    return this.currentUser?.roles.includes(role) ?? false;
  }

  hasAnyRole(roles: string[]): boolean {

    return roles.some(role => this.hasRole(role));
  }

  getHomeRoute(): string {

    if (this.hasRole('Manager')) {
      return '/manager/dashboard';
    }

    if (this.hasRole('ClaimsOfficer')) {
      return '/officer/worklist';
    }

    if (this.hasRole('Claimant')) {
      return '/claimant/dashboard';
    }

    return '/login';
  }

  logout(): void {

    this.clearSession();

    void this.router.navigateByUrl('/login');
  }

  private restoreSession(): AuthSession | null {

    try {

      const storedSession = sessionStorage.getItem(
        this.storageKey
      );

      if (!storedSession) {
        return null;
      }

      const session = JSON.parse(
        storedSession
      ) as AuthSession;

      const expiry = Date.parse(session.expiresAtUtc);

      if (
        !session.accessToken ||
        !session.tokenType ||
        !session.user?.userId ||
        !Array.isArray(session.user.roles) ||
        !Number.isFinite(expiry) ||
        expiry <= Date.now()
      ) {
        sessionStorage.removeItem(this.storageKey);
        return null;
      }

      return session;

    } catch {

      sessionStorage.removeItem(this.storageKey);
      return null;
    }
  }

  private clearSession(): void {

    this.session = null;

    sessionStorage.removeItem(this.storageKey);

    this.currentUserSubject.next(null);
  }
}

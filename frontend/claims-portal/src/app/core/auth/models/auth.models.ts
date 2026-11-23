export type AppRole =
  | 'Claimant'
  | 'ClaimsOfficer'
  | 'Manager';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResult {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface AuthenticatedUser {
  userId: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface AuthSession {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  user: AuthenticatedUser;
}

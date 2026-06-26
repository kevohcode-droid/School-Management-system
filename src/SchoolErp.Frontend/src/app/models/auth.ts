export interface LoginRequest {
  tenantCode: string;
  email: string;
  password: string;
}

export interface RegisterRequest {
  tenantCode: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
  tenantId: string;
  roles: string[];
}

export interface CurrentUser {
  userId: string;
  userName: string;
  tenantId: string;
  roles: string[];
}

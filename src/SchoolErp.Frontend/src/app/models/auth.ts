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

export interface GoogleLoginRequest {
  token: string;
  tenantCode: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
  fullName: string;
  tenantId: string;
  roles: string[];
}

export interface CurrentUser {
  userId: string;
  userName: string;
  fullName?: string;
  tenantId: string;
  tenantName?: string;
  tenantCode?: string;
  roles: string[];
}

export interface UserProfile {
  fullName: string;
  email: string;
  phone?: string;
  role: string;
  school: string;
  tenantCode?: string;
  username: string;
  lastLogin?: string;
  dateJoined?: string;
  address?: string;
}

export interface UpdateProfileRequest {
  fullName: string;
  phone?: string;
  address?: string;
}

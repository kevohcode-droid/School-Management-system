import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { LoginRequest, RegisterRequest, AuthResponse, CurrentUser, GoogleLoginRequest, UserProfile, UpdateProfileRequest } from '../models/auth';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private apiService: ApiService) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.apiService.post<AuthResponse>('/auth/login', request);
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.apiService.post<AuthResponse>('/auth/register', request);
  }

  getCurrentUser(): Observable<CurrentUser> {
    return this.apiService.get<CurrentUser>('/auth/me');
  }

  getUserProfile(): Observable<UserProfile> {
    return this.apiService.get<UserProfile>('/users/me');
  }

  updateProfile(request: UpdateProfileRequest): Observable<any> {
    return this.apiService.put('/users/me', request);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<any> {
    return this.apiService.post('/auth/change-password', {
      currentPassword,
      newPassword
    }).pipe(tap(() => localStorage.removeItem('mustChangePassword')));
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    localStorage.removeItem('mustChangePassword');
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  saveToken(token: string): void {
    localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  saveCurrentUser(user: CurrentUser): void {
    localStorage.setItem('currentUser', JSON.stringify(user));
  }

  getCurrentUserFromStorage(): CurrentUser | null {
    const user = localStorage.getItem('currentUser');
    return user ? JSON.parse(user) : null;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUserFromStorage();
    return user ? user.roles.includes(role) : false;
  }

  forgotPassword(email: string): Observable<any> {
    return this.apiService.post<any>('/auth/forgot-password', { email });
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<any> {
    return this.apiService.post<any>('/auth/reset-password', { email, token, newPassword });
  }

  googleLogin(request: GoogleLoginRequest): Observable<AuthResponse> {
    return this.apiService.post<AuthResponse>('/auth/google-signup', request);
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUserFromStorage();
    return user ? roles.some(role => user.roles.includes(role)) : false;
  }

  getLinkedStudentIds(): string[] {
    const user = this.getCurrentUserFromStorage();
    if (user?.linkedStudentIds?.length) {
      return user.linkedStudentIds;
    }

    const payload = this.getTokenPayload();
    const claimValue = payload?.linkedStudentIds;
    if (!claimValue) {
      return [];
    }

    return String(claimValue)
      .split(',')
      .map(id => id.trim())
      .filter(Boolean);
  }

  private getTokenPayload(): any | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    const [, payload] = token.split('.');
    if (!payload) {
      return null;
    }

    try {
      return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
    } catch {
      return null;
    }
  }
}

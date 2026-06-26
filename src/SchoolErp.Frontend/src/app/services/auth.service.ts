import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { LoginRequest, RegisterRequest, AuthResponse, CurrentUser } from '../models/auth';

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

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
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

  hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUserFromStorage();
    return user ? roles.some(role => user.roles.includes(role)) : false;
  }
}

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  StudentProfile,
  StudentPerformanceResponse,
  StudentAttendanceResponse,
  StudentFeesResponse,
  StudentDashboard
} from '../models/student-portal';

/**
 * All calls go to /api/student-portal/* which enforces:
 *   - Role = Student
 *   - Ownership: Student.UserId == authenticated user's id
 *   - TenantId isolation
 * No mutation methods are provided — this portal is read-only.
 */
@Injectable({ providedIn: 'root' })
export class StudentPortalService {
  private readonly BASE = '/student-portal';

  constructor(private api: ApiService) {}

  getDashboard(): Observable<StudentDashboard> {
    return this.api.get<StudentDashboard>(`${this.BASE}/dashboard`);
  }

  getProfile(): Observable<StudentProfile> {
    return this.api.get<StudentProfile>(`${this.BASE}/me`);
  }

  getPerformance(): Observable<StudentPerformanceResponse> {
    return this.api.get<StudentPerformanceResponse>(`${this.BASE}/performance`);
  }

  getAttendance(year?: number, month?: number): Observable<StudentAttendanceResponse> {
    const params: Record<string, any> = {};
    if (year) params['year'] = year;
    if (month) params['month'] = month;
    return this.api.get<StudentAttendanceResponse>(`${this.BASE}/attendance`, params);
  }

  getFees(): Observable<StudentFeesResponse> {
    return this.api.get<StudentFeesResponse>(`${this.BASE}/fees`);
  }
}

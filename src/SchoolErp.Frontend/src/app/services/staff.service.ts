import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { ApiService } from './api.service';
import { Staff, CreateStaffRequest } from '../models/staff';

@Injectable({
  providedIn: 'root'
})
export class StaffService {
  constructor(private apiService: ApiService) {}

  getStaff(): Observable<Staff[]> {
    return this.apiService.get<Staff[]>('/staff');
  }

  createStaff(request: CreateStaffRequest): Observable<Staff> {
    return this.apiService.post<Staff>('/staff', request);
  }

  updateStaff(id: string, request: CreateStaffRequest): Observable<Staff> {
    return this.apiService.put<Staff>(`/staff/${id}`, request);
  }

  deleteStaff(id: string): Observable<void> {
    return this.apiService.delete<void>(`/staff/${id}`);
  }

  exportStaff(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first to export staff.');
      return;
    }

    this.apiService.download('/staff/export', 'Staff.xlsx');
  }

  importStaff(file: File): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first to import staff.');
      return of(null);
    }

    const formData = new FormData();
    formData.append('file', file);

    return this.apiService.upload<any>('/staff/import', formData);
  }

  downloadTemplate(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first to download the template.');
      return;
    }

    this.apiService.download('/staff/template', 'Staff-Template.xlsx');
  }
}
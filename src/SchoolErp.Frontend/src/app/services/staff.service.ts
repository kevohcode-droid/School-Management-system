import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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
    const headers = token ? { Authorization: `Bearer ${token}` } : undefined;

    const link = document.createElement('a');
    link.href = 'http://localhost:5110/api/staff/export';
    if (headers?.Authorization) {
      link.setAttribute('headers', JSON.stringify(headers));
    }
    link.download = 'Staff.xlsx';
    link.click();
  }

  importStaff(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);

    const token = localStorage.getItem('token');
    const headers = token ? { Authorization: `Bearer ${token}` } : undefined;

    return this.apiService.upload<any>('/staff/import', formData);
  }

  downloadTemplate(): void {
    const link = document.createElement('a');
    link.href = 'http://localhost:5110/api/staff/template';
    link.download = 'Staff-Template.xlsx';
    link.click();
  }
}
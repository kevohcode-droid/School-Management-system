import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class SectionsService {
  constructor(private apiService: ApiService) {}

  getSections(): Observable<any[]> {
    return this.apiService.get<any[]>('/sections');
  }

  getSection(id: string): Observable<any> {
    return this.apiService.get<any>(`/sections/${id}`);
  }

  createSection(data: any): Observable<any> {
    return this.apiService.post<any>('/sections', data);
  }

  updateSection(id: string, data: any): Observable<any> {
    return this.apiService.put<any>(`/sections/${id}`, data);
  }

  deleteSection(id: string): Observable<void> {
    return this.apiService.delete<void>(`/sections/${id}`);
  }

  exportSections(): void {
    const token = localStorage.getItem('token');
    const headers = token ? { Authorization: `Bearer ${token}` } : undefined;

    const link = document.createElement('a');
    link.href = 'http://localhost:5110/api/sections/export';
    if (headers?.Authorization) {
      link.setAttribute('headers', JSON.stringify(headers));
    }
    link.download = 'Sections.xlsx';
    link.click();
  }

  importSections(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);

    const token = localStorage.getItem('token');
    const headers = token ? { Authorization: `Bearer ${token}` } : undefined;

    return this.apiService.upload<any>('/sections/import', formData);
  }

  downloadTemplate(): void {
    const link = document.createElement('a');
    link.href = 'http://localhost:5110/api/sections/template';
    link.download = 'Sections-Template.xlsx';
    link.click();
  }
}
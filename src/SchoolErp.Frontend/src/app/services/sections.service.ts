import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
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
    if (!token) {
      alert('Please log in first to export sections.');
      return;
    }

    this.apiService.download('/sections/export', 'Sections.xlsx');
  }

  importSections(file: File): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first to import sections.');
      return of(null);
    }

    const formData = new FormData();
    formData.append('file', file);

    return this.apiService.upload<any>('/sections/import', formData);
  }

  downloadTemplate(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first to download the template.');
      return;
    }

    this.apiService.download('/sections/template', 'Sections-Template.xlsx');
  }
}
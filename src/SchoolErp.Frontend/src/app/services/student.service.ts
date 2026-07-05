import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Student, CreateStudentRequest, UpdateStudentRequest } from '../models/student';

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  constructor(private apiService: ApiService) {}

  getStudents(): Observable<Student[]> {
    return this.apiService.get<Student[]>('/students');
  }

  getStudent(id: string): Observable<Student> {
    return this.apiService.get<Student>(`/students/${id}`);
  }

  createStudent(request: CreateStudentRequest): Observable<Student> {
    return this.apiService.post<Student>('/students', request);
  }

  updateStudent(id: string, request: UpdateStudentRequest): Observable<Student> {
    return this.apiService.put<Student>(`/students/${id}`, request);
  }

  deleteStudent(id: string): Observable<void> {
    return this.apiService.delete<void>(`/students/${id}`);
  }

  exportStudents(): void {
    const token = localStorage.getItem('token');
    const headers = token ? { Authorization: `Bearer ${token}` } : undefined;
    
    const link = document.createElement('a');
    link.href = 'http://localhost:5110/api/students/export';
    if (headers?.Authorization) {
      link.setAttribute('headers', JSON.stringify(headers));
    }
    link.download = 'Students.xlsx';
    link.click();
  }

  downloadTemplate(): void {
    const link = document.createElement('a');
    link.href = 'http://localhost:5110/api/students/template';
    link.download = 'Students-Template.xlsx';
    link.click();
  }

  importStudents(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    
    const token = localStorage.getItem('token');
    const headers = token ? { Authorization: `Bearer ${token}` } : undefined;
    
    return this.apiService.upload<any>('/students/import', formData);
  }
}

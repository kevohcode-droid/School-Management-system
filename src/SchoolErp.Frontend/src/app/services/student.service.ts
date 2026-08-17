import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
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
    if (!token) {
      alert('Please log in first to export students.');
      return;
    }

    this.apiService.download('/students/export', 'Students.xlsx');
  }

  downloadTemplate(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first to download the template.');
      return;
    }

    this.apiService.download('/students/template', 'Students-Template.xlsx');
  }

importStudents(file: File): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first to import students.');
      return of(null);
    }

    const formData = new FormData();
    formData.append('file', file);

    return this.apiService.upload<any>('/students/import', formData);
  }
}

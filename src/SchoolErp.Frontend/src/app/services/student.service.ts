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
}

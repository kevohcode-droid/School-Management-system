import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { BulkSaveResponse, SaveStudentMarkItemDto, StudentMarkDto } from '../models/performance';

@Injectable({
  providedIn: 'root'
})
export class PerformanceService {
  constructor(private apiService: ApiService) {}

  bulkSaveMarks(marks: SaveStudentMarkItemDto[]): Observable<BulkSaveResponse> {
    return this.apiService.post<BulkSaveResponse>('/performance/bulk-save', marks);
  }

  getStudentPerformance(studentId: string): Observable<StudentMarkDto[]> {
    return this.apiService.get<StudentMarkDto[]>(`/performance/student/${studentId}`);
  }

  getClassPerformance(classId: string, term?: string): Observable<StudentMarkDto[]> {
    const params = term ? { term } : {};
    return this.apiService.get<StudentMarkDto[]>(`/performance/class/${classId}`, params);
  }

  reviewMark(markId: string, status: string, comment?: string): Observable<{ message: string; status: string }> {
    return this.apiService.post<{ message: string; status: string }>(`/performance/mark/${markId}/review`, { status, comment });
  }

  reopenMark(markId: string): Observable<{ message: string; status: string }> {
    return this.apiService.post<{ message: string; status: string }>(`/performance/mark/${markId}/reopen`, {});
  }
}

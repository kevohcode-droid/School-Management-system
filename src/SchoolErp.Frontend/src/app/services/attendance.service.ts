import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  AttendanceRecord, 
  BulkMarkAttendanceDto, 
  StudentAttendanceDto 
} from '../models/attendance';

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private apiUrl = 'api/attendance';

  constructor(private http: HttpClient) {}

  getDailyAttendance(classId: string, date: string): Observable<AttendanceRecord[]> {
    return this.http.get<AttendanceRecord[]>(`${this.apiUrl}/daily`, {
      params: { classId, date }
    });
  }

  getClassStudents(classId: string): Observable<StudentAttendanceDto[]> {
    return this.http.get<StudentAttendanceDto[]>(`${this.apiUrl}/class-students`, {
      params: { classId }
    });
  }

  bulkMarkAttendance(dto: BulkMarkAttendanceDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/bulk`, dto);
  }
}

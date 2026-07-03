import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AttendanceService } from '../../services/attendance.service';
import { 
  AttendanceRecord, 
  AttendanceStatus, 
  StudentAttendanceDto,
  BulkMarkAttendanceDto 
} from '../../models/attendance';

@Component({
  selector: 'app-daily-attendance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './daily-attendance.html',
  styleUrls: ['./daily-attendance.css']
})
export class DailyAttendanceComponent implements OnInit {
  selectedDate = new Date().toISOString().split('T')[0];
  selectedClassId = '';
  attendanceList: AttendanceRecord[] = [];
  studentsList: StudentAttendanceDto[] = [];
  isSaving = false;
  isLoading = false;
  errorMessage = '';

  // Class options - should be loaded from API in a real implementation
  classOptions = [
    { id: 'class-1', name: 'Class 10-A' },
    { id: 'class-2', name: 'Class 10-B' },
    { id: 'class-3', name: 'Class 9-A' }
  ];

  constructor(
    private attendanceService: AttendanceService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Load initial data if a class is pre-selected
  }

  loadStudents(): void {
    if (!this.selectedClassId) return;
    
    this.isLoading = true;
    this.errorMessage = '';
    
    this.attendanceService.getClassStudents(this.selectedClassId)
      .subscribe({
        next: (data) => {
          this.studentsList = data;
          // Initialize attendance records with default Present status
          this.attendanceList = data.map(student => ({
            studentId: student.studentId,
            studentName: student.studentName,
            status: AttendanceStatus.Present,
            remarks: ''
          }));
          this.isLoading = false;
          
          // Now load existing attendance for this date
          this.loadExistingAttendance();
        },
        error: (err) => {
          console.error('Failed to load students', err);
          this.errorMessage = 'Failed to load students. Please try again.';
          this.isLoading = false;
        }
      });
  }

  loadExistingAttendance(): void {
    this.attendanceService.getDailyAttendance(this.selectedClassId, this.selectedDate)
      .subscribe({
        next: (data) => {
          // Merge existing attendance with student list
          data.forEach(existing => {
            const record = this.attendanceList.find(r => r.studentId === existing.studentId);
            if (record) {
              record.status = existing.status;
              record.remarks = existing.remarks;
            }
          });
        },
        error: (err) => {
          console.error('Failed to load existing attendance', err);
          // Not critical - continue with default values
        }
      });
  }

  updateStatus(studentId: string, status: AttendanceStatus): void {
    const record = this.attendanceList.find(r => r.studentId === studentId);
    if (record) record.status = status;
  }

  updateRemarks(studentId: string, remarks: string): void {
    const record = this.attendanceList.find(r => r.studentId === studentId);
    if (record) record.remarks = remarks;
  }

  saveAll(): void {
    if (!this.selectedClassId) {
      alert('Please select a class first');
      return;
    }

    this.isSaving = true;
    
    const bulkDto: BulkMarkAttendanceDto = {
      classId: this.selectedClassId,
      date: this.selectedDate,
      records: this.attendanceList.map(record => ({
        studentId: record.studentId,
        date: this.selectedDate,
        status: record.status,
        remarks: record.remarks
      }))
    };
    
    this.attendanceService.bulkMarkAttendance(bulkDto)
      .subscribe({
        next: () => {
          alert('Attendance saved successfully!');
          this.isSaving = false;
        },
        error: (err) => {
          console.error('Error saving attendance', err);
          alert('Error saving attendance. Please try again.');
          this.isSaving = false;
        }
      });
  }

  getStatusLabel(status: AttendanceStatus): string {
    switch (status) {
      case AttendanceStatus.Present: return 'Present';
      case AttendanceStatus.Absent: return 'Absent';
      case AttendanceStatus.Late: return 'Late';
      case AttendanceStatus.Excused: return 'Excused';
      default: return 'Unknown';
    }
  }

  getStatusClass(status: AttendanceStatus): string {
    switch (status) {
      case AttendanceStatus.Present: return 'status-present';
      case AttendanceStatus.Absent: return 'status-absent';
      case AttendanceStatus.Late: return 'status-late';
      case AttendanceStatus.Excused: return 'status-excused';
      default: return '';
    }
  }

  navigateToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }
}

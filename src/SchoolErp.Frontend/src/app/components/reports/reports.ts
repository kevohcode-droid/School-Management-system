import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ApiService } from '../../services/api.service';

type ReportType = 'students' | 'staff' | 'classes' | 'attendance';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.html',
  styleUrls: ['./reports.css']
})
export class ReportsComponent implements OnInit {
  activeReport: ReportType = 'students';
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  studentsList: any[] = [];
  studentSearch = '';

  attendanceFilters = {
    dateFrom: new Date().toISOString().split('T')[0],
    dateTo: new Date().toISOString().split('T')[0],
    classId: '',
    status: ''
  };
  attendanceList: any[] = [];
  classesList: any[] = [];

  attendanceStatuses = [
    { value: '', label: 'All Statuses' },
    { value: 'Present', label: 'Present' },
    { value: 'Absent', label: 'Absent' },
    { value: 'Late', label: 'Late' },
    { value: 'Excused', label: 'Excused' }
  ];

  constructor(
    public authService: AuthService,
    private apiService: ApiService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    const reportType = this.route.snapshot.queryParams['type'];
    if (['students', 'staff', 'classes', 'attendance'].includes(reportType)) {
      this.activeReport = reportType;
    }

    this.loadStudents();
    this.loadClasses();
    if (this.activeReport === 'attendance') {
      this.loadAttendance();
    }
  }

  setActiveReport(type: ReportType): void {
    this.activeReport = type;
    this.errorMessage = '';
    this.successMessage = '';

    if (type === 'students') {
      this.loadStudents();
    } else if (type === 'staff') {
      this.successMessage = 'Use the Staff page for staff management and exports.';
    } else if (type === 'classes') {
      this.loadClasses();
    } else if (type === 'attendance') {
      this.loadAttendance();
    }
  }

  loadStudents(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.apiService.get<any[]>(`/students`).subscribe({
      next: (data) => {
        this.studentsList = data || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load students', err);
        this.errorMessage = 'Failed to load students';
        this.isLoading = false;
      }
    });
  }

  loadClasses(): void {
    this.apiService.get<any[]>(`/sections`).subscribe({
      next: (data) => {
        this.classesList = (data || []).map((c: any) => ({
          id: c.id,
          name: c.name,
          gradeLevel: c.gradeLevel
        }));
      },
      error: (err) => {
        console.error('Failed to load classes', err);
      }
    });
  }

  loadAttendance(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const params: any = {};
    if (this.attendanceFilters.dateFrom) params.dateFrom = this.attendanceFilters.dateFrom;
    if (this.attendanceFilters.dateTo) params.dateTo = this.attendanceFilters.dateTo;

    this.apiService.get<any[]>(`/attendance/all`, params).subscribe({
      next: (data) => {
        this.attendanceList = data || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load attendance', err);
        this.errorMessage = 'Failed to load attendance records';
        this.isLoading = false;
      }
    });
  }

  filterAttendance(): void {
    this.loadAttendance();
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  downloadStudentsExcel(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first.');
      return;
    }

    fetch('/api/students/export', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
      }
    })
      .then(response => {
        if (!response.ok) throw new Error('Export failed');
        return response.blob();
      })
      .then(blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'Students.xlsx';
        link.click();
        window.URL.revokeObjectURL(url);
      })
      .catch(err => {
        console.error(err);
        alert('Failed to export students. Please try again.');
      });
  }

  downloadStaffExcel(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first.');
      return;
    }

    fetch('/api/staff/export', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
      }
    })
      .then(response => {
        if (!response.ok) throw new Error('Export failed');
        return response.blob();
      })
      .then(blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'Staff.xlsx';
        link.click();
        window.URL.revokeObjectURL(url);
      })
      .catch(err => {
        console.error(err);
        alert('Failed to export staff. Please try again.');
      });
  }

  downloadClassesExcel(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first.');
      return;
    }

    fetch('/api/sections/export', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
      }
    })
      .then(response => {
        if (!response.ok) throw new Error('Export failed');
        return response.blob();
      })
      .then(blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'Classes.xlsx';
        link.click();
        window.URL.revokeObjectURL(url);
      })
      .catch(err => {
        console.error(err);
        alert('Failed to export classes. Please try again.');
      });
  }

  downloadAttendanceExcel(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Please log in first.');
      return;
    }

    const params = new URLSearchParams();
    if (this.attendanceFilters.dateFrom) params.append('dateFrom', this.attendanceFilters.dateFrom);
    if (this.attendanceFilters.dateTo) params.append('dateTo', this.attendanceFilters.dateTo);
    if (this.attendanceFilters.classId) params.append('classId', this.attendanceFilters.classId);

    fetch(`/api/attendance/export?${params.toString()}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
      }
    })
      .then(response => {
        if (!response.ok) throw new Error('Export failed');
        return response.blob();
      })
      .then(blob => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'Attendance.xlsx';
        link.click();
        window.URL.revokeObjectURL(url);
      })
      .catch(err => {
        console.error(err);
        alert('Failed to export attendance. Please try again.');
      });
  }

  getFilteredStudents(): any[] {
    if (!this.studentSearch) return this.studentsList;
    const searchLower = this.studentSearch.toLowerCase();
    return this.studentsList.filter(s =>
      (s.admissionNumber || '').toLowerCase().includes(searchLower) ||
      (s.firstName || '').toLowerCase().includes(searchLower) ||
      (s.lastName || '').toLowerCase().includes(searchLower)
    );
  }

  getStatusLabel(status: number | string): string {
    if (typeof status === 'number') {
      switch (status) {
        case 1: return 'Present';
        case 2: return 'Absent';
        case 3: return 'Late';
        case 4: return 'Excused';
        default: return 'Unknown';
      }
    }
    return status;
  }

  getStatusClass(status: number | string): string {
    if (typeof status === 'number') {
      switch (status) {
        case 1: return 'status-present';
        case 2: return 'status-absent';
        case 3: return 'status-late';
        case 4: return 'status-excused';
        default: return '';
      }
    }
    return `status-${status.toLowerCase()}`;
  }
}
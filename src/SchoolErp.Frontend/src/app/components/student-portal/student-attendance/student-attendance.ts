import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentPortalService } from '../../../services/student-portal.service';
import {
  StudentAttendanceResponse,
  StudentAttendanceRecord,
  AttendanceSummary
} from '../../../models/student-portal';

@Component({
  selector: 'app-student-attendance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './student-attendance.html',
  styleUrls: ['./student-attendance.css']
})
export class StudentAttendanceComponent implements OnInit {
  attendanceData: StudentAttendanceResponse | null = null;
  selectedYear: number = new Date().getFullYear();
  selectedMonth: number = new Date().getMonth() + 1;
  isLoading = false;
  error: string | null = null;

  readonly monthOptions: { value: number; label: string }[] = [
    { value: 1,  label: 'January'   },
    { value: 2,  label: 'February'  },
    { value: 3,  label: 'March'     },
    { value: 4,  label: 'April'     },
    { value: 5,  label: 'May'       },
    { value: 6,  label: 'June'      },
    { value: 7,  label: 'July'      },
    { value: 8,  label: 'August'    },
    { value: 9,  label: 'September' },
    { value: 10, label: 'October'   },
    { value: 11, label: 'November'  },
    { value: 12, label: 'December'  }
  ];

  constructor(private studentPortalService: StudentPortalService) {}

  ngOnInit(): void {
    this.loadAttendance();
  }

  loadAttendance(): void {
    this.isLoading = true;
    this.error = null;

    this.studentPortalService.getAttendance(this.selectedYear, this.selectedMonth).subscribe({
      next: (data) => {
        this.attendanceData = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load attendance data. Please try again.';
        this.isLoading = false;
        console.error('Attendance load error:', err);
      }
    });
  }

  onFilterChange(): void {
    this.loadAttendance();
  }

  getStatusBadgeClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'present': return 'badge-success';
      case 'absent':  return 'badge-danger';
      case 'late':    return 'badge-warning';
      case 'excused': return 'badge-info';
      default:        return 'badge-secondary';
    }
  }

  getDayOfWeek(dateStr: string): string {
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const date = new Date(dateStr);
    return days[date.getDay()];
  }

  get summary(): AttendanceSummary | null {
    return this.attendanceData?.summary ?? null;
  }

  get records(): StudentAttendanceRecord[] {
    return this.attendanceData?.records ?? [];
  }
}

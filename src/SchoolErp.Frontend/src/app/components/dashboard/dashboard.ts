import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { DashboardService } from '../../services/dashboard.service';
import { StudentService } from '../../services/student.service';
import { StaffService } from '../../services/staff.service';
import { SectionsService } from '../../services/sections.service';
import { DashboardSummary } from '../../models/dashboard';
import { CurrentUser } from '../../models/auth';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  currentUser: CurrentUser | null = null;
  currentDate: Date = new Date();
  
  notificationCount: number = 0;
  showNotifications: boolean = false;
  showProfile: boolean = false;

  notifications: string[] = [];
  activities: string[] = [
    'Student John registered',
    'Attendance submitted',
    'Teacher Mary added',
    'Fee payment received',
    'Class 8A created'
  ];

  systemStatus: string[] = [
    'Database Connected',
    'API Running',
    'Backup Completed',
    'Email Service Active'
  ];

  enrollmentData: { month: string; percentage: number }[] = [
    { month: 'Jan', percentage: 30 },
    { month: 'Feb', percentage: 45 },
    { month: 'Mar', percentage: 65 },
    { month: 'Apr', percentage: 80 },
    { month: 'May', percentage: 95 },
    { month: 'Jun', percentage: 100 }
  ];

  summary: DashboardSummary | null = null;
  isLoadingSummary = false;

  get lastLogin(): string {
    if (!this.summary?.lastLogin) return 'Never';
    const date = new Date(this.summary.lastLogin);
    return date.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  constructor(
    public authService: AuthService,
    private router: Router,
    private dashboardService: DashboardService,
    private studentService: StudentService,
    private staffService: StaffService,
    private sectionsService: SectionsService
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    const cachedUser = this.authService.getCurrentUserFromStorage();
    if (cachedUser) {
      this.currentUser = cachedUser;
    }

    this.authService.getCurrentUser().subscribe({
      next: (user: CurrentUser) => {
        this.currentUser = user;
        this.authService.saveCurrentUser(user);
      },
      error: () => {
        if (!this.currentUser) {
          this.router.navigate(['/login']);
        }
      }
    });

    this.loadDashboardSummary();
  }

  loadDashboardSummary(): void {
    this.isLoadingSummary = true;
    this.dashboardService.getSummary().subscribe({
      next: (data) => {
        this.summary = data;
        this.notificationCount = data.notifications;
        this.isLoadingSummary = false;
      },
      error: (err) => {
        console.error('Failed to load dashboard summary', err);
        this.isLoadingSummary = false;
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
    this.showProfile = false;
  }

  toggleProfile(): void {
    this.showProfile = !this.showProfile;
    this.showNotifications = false;
  }

  viewProfile(): void {
    this.showProfile = false;
    alert('Profile page coming soon.');
  }

  changePassword(): void {
    this.showProfile = false;
    this.router.navigate(['/settings']);
  }

  openPreferences(): void {
    this.showProfile = false;
    this.router.navigate(['/settings']);
  }

  openAuditLogs(): void {
    this.showProfile = false;
    alert('Audit Logs page coming soon.');
  }

  navigateToStudents(): void {
    this.router.navigate(['/students']);
  }

  navigateToTenants(): void {
    this.router.navigate(['/tenants']);
  }

  navigateToStaff(): void {
    this.router.navigate(['/staff']);
  }

  navigateToClasses(): void {
    this.router.navigate(['/classes']);
  }

  navigateToAttendance(): void {
    this.router.navigate(['/attendance']);
  }

  navigateToSettings(): void {
    this.router.navigate(['/settings']);
  }

  addStudent(): void {
    this.router.navigate(['/students']);
  }

  addStaff(): void {
    this.router.navigate(['/staff']);
  }

  createClass(): void {
    this.router.navigate(['/classes']);
  }

  markAttendance(): void {
    this.router.navigate(['/attendance']);
  }

  importExcel(): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.xlsx,.xls,.csv';
    input.onchange = (e: any) => {
      const file = e.target.files[0];
      if (file) {
        this.studentService.importStudents(file).subscribe({
          next: (result) => alert(`Imported ${result.imported || 0} students successfully`),
          error: (err) => alert('Import failed: ' + err.message)
        });
      }
    };
    input.click();
  }

  exportReport(): void {
    this.studentService.exportStudents();
  }

  importStaffExcel(): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.xlsx,.xls,.csv';
    input.onchange = (e: any) => {
      const file = e.target.files[0];
      if (file) {
        this.staffService.importStaff(file).subscribe({
          next: (result) => alert(`Imported ${result.imported || 0} staff successfully`),
          error: (err) => alert('Import failed: ' + err.message)
        });
      }
    };
    input.click();
  }

  exportStaffReport(): void {
    this.staffService.exportStaff();
  }

  importClassExcel(): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.xlsx,.xls,.csv';
    input.onchange = (e: any) => {
      const file = e.target.files[0];
      if (file) {
        this.sectionsService.importSections(file).subscribe({
          next: (result) => alert(`Imported ${result.imported || 0} sections successfully`),
          error: (err) => alert('Import failed: ' + err.message)
        });
      }
    };
    input.click();
  }

  exportClassReport(): void {
    this.sectionsService.exportSections();
  }
}
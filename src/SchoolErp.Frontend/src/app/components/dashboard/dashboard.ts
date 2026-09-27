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

  // Header/notification/profile UI is handled by the global TopNavbarComponent.
  // No showProfile / showNotifications / notificationCount needed here.

  summary: DashboardSummary | null = null;
  isLoadingSummary = false;

  get isTeacher(): boolean {
    return this.currentUser?.roles?.includes('Teacher') ?? false;
  }

  get isAdmin(): boolean {
    return this.currentUser?.roles?.some(role => role === 'Admin' || role === 'SuperAdmin') ?? false;
  }

  get isAccountant(): boolean {
    return this.currentUser?.roles?.includes('Accountant') ?? false;
  }

  get canUseAcademicTools(): boolean {
    return this.isTeacher || this.isAdmin;
  }

  get attendancePresent(): number {
    const percentage = this.summary?.attendancePercentage;
    return percentage == null ? 0 : Math.max(0, Math.min(100, percentage));
  }

  get attendanceAbsent(): number {
    return 100 - this.attendancePresent;
  }

  get dashboardStatus(): string {
    if (this.isLoadingSummary) return 'Checking dashboard data';
    return this.summary ? 'Dashboard data available' : 'Dashboard data unavailable';
  }

  get lastLogin(): string {
    if (!this.summary?.lastLogin) return 'Never';
    const date = new Date(this.summary.lastLogin);
    return date.toLocaleString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
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
    if (cachedUser) this.currentUser = cachedUser;

    this.authService.getCurrentUser().subscribe({
      next: (user: CurrentUser) => {
        this.currentUser = user;
        this.authService.saveCurrentUser(user);
      },
      error: () => {
        if (!this.currentUser) this.router.navigate(['/login']);
      }
    });

    this.loadDashboardSummary();
  }

  loadDashboardSummary(): void {
    this.isLoadingSummary = true;
    this.dashboardService.getSummary().subscribe({
      next: (data) => {
        this.summary = data;
        this.isLoadingSummary = false;
      },
      error: (err) => {
        console.error('Failed to load dashboard summary', err);
        this.isLoadingSummary = false;
      }
    });
  }

  // ── Navigation helpers ──────────────────────────────────
  logout(): void { this.authService.logout(); this.router.navigate(['/login']); }
  navigateToStudents(): void   { this.router.navigate(['/students']); }
  navigateToTenants(): void    { this.router.navigate(['/tenants']); }
  navigateToStaff(): void      { this.router.navigate(['/staff']); }
  navigateToClasses(): void    { this.router.navigate(['/classes']); }
  navigateToAttendance(): void { this.router.navigate(['/attendance']); }
  navigateToSettings(): void   { this.router.navigate(['/settings']); }
  navigateTo(route: string): void { this.router.navigate([route]); }
  navigateToPerformance(): void { this.router.navigate(['/academics/performance']); }
  addStudent(): void   { this.router.navigate(['/students']); }
  addStaff(): void     { this.router.navigate(['/staff']); }
  createClass(): void  { this.router.navigate(['/classes']); }
  markAttendance(): void { this.router.navigate(['/attendance']); }
  openStudentPerformance(): void { this.router.navigate(['/academics/performance']); }

  exportReport(): void      { this.studentService.exportStudents(); }
  exportStaffReport(): void { this.staffService.exportStaff(); }
  exportClassReport(): void { this.sectionsService.exportSections(); }

  importExcel(): void {
    this.openFilePicker('.xlsx,.xls,.csv', (file) => {
      this.studentService.importStudents(file).subscribe({
        next: (r) => alert(`Imported ${r.imported || 0} students successfully`),
        error: (e) => alert('Import failed: ' + e.message)
      });
    });
  }

  importStaffExcel(): void {
    this.openFilePicker('.xlsx,.xls,.csv', (file) => {
      this.staffService.importStaff(file).subscribe({
        next: (r) => alert(`Imported ${r.imported || 0} staff successfully`),
        error: (e) => alert('Import failed: ' + e.message)
      });
    });
  }

  importClassExcel(): void {
    this.openFilePicker('.xlsx,.xls,.csv', (file) => {
      this.sectionsService.importSections(file).subscribe({
        next: (r) => alert(`Imported ${r.imported || 0} sections successfully`),
        error: (e) => alert('Import failed: ' + e.message)
      });
    });
  }

  private openFilePicker(accept: string, callback: (f: File) => void): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = accept;
    input.onchange = (e: any) => {
      const file: File = e.target.files[0];
      if (file) callback(file);
    };
    input.click();
  }
}

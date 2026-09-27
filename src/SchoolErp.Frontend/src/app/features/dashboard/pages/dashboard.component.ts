import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { DashboardService } from '../../../services/dashboard.service';
import { StudentService } from '../../../services/student.service';
import { StaffService } from '../../../services/staff.service';
import { SectionsService } from '../../../services/sections.service';
import { DashboardSummary } from '../../../models/dashboard';
import { CurrentUser } from '../../../models/auth';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';

interface MetricCard {
  title: string;
  value: number | string;
  trend?: number;
  previousValue?: number;
  icon: string;
  color: string;
  action?: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  currentUser: CurrentUser | null = null;
  currentDate: Date = new Date();

  notificationCount: number = 0;
  showNotifications: boolean = false;

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

  summary: DashboardSummary | null = null;
  isLoadingSummary = false;
  isDarkMode = false;
  hasPendingTasks = false;
  hasNewMessages = false;

  metricCards: MetricCard[] = [];
  private subscription: Subscription = new Subscription();

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

    this.subscription = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      const cachedUser = this.authService.getCurrentUserFromStorage();
      if (cachedUser && !this.currentUser) {
        this.currentUser = cachedUser;
      }
    });

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
    this.buildMetricCards();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  buildMetricCards(): MetricCard[] {
    this.metricCards = [
      {
        title: 'Total Students',
        value: this.summary?.totalStudents ?? 0,
        trend: 5.2,
        previousValue: (this.summary?.totalStudents ?? 0) - 10,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">man</span>‍<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">school</span>',
        color: '#3b82f6',
        action: 'students'
      },
      {
        title: 'Total Staff',
        value: this.summary?.totalStaff ?? 0,
        trend: 2.1,
        previousValue: (this.summary?.totalStaff ?? 0) - 2,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">woman</span>‍<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">school</span>',
        color: '#10b981',
        action: 'staff'
      },
      {
        title: 'Total Teachers',
        value: Math.round((this.summary?.totalStaff ?? 0) * 0.7),
        trend: 3.5,
        previousValue: Math.round(((this.summary?.totalStaff ?? 0) - 2) * 0.7),
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">woman</span>‍<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">school</span>',
        color: '#059669',
        action: 'staff'
      },
      {
        title: 'Total Classes',
        value: this.summary?.totalClasses ?? 0,
        trend: 4.8,
        previousValue: (this.summary?.totalClasses ?? 0) - 1,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">book</span>',
        color: '#8b5cf6',
        action: 'classes'
      },
      {
        title: 'Total Subjects',
        value: 24,
        trend: 2.5,
        previousValue: 23,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">menu_book</span>',
        color: '#06b6d4',
        action: 'subjects'
      },
      {
        title: 'Attendance Today',
        value: this.summary?.attendancePercentage != null ? `${Math.round(this.summary.attendancePercentage)}%` : 'N/A',
        trend: 1.2,
        previousValue: this.summary?.attendancePercentage != null ? (this.summary.attendancePercentage - 1.2) : undefined,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">calendar_month</span>',
        color: '#f59e0b'
      },
      {
        title: 'Fee Collection Today',
        value: `KSh ${(this.summary?.todayCollections ?? 0).toLocaleString()}`,
        trend: 7.3,
        previousValue: (this.summary?.todayCollections ?? 0) - 5000,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">payments</span>',
        color: '#10b981'
      },
      {
        title: 'Outstanding Fees',
        value: `KSh ${(this.summary?.pendingFees ?? 0).toLocaleString()}`,
        trend: -2.1,
        previousValue: (this.summary?.pendingFees ?? 0) + 3000,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">money_off</span>',
        color: '#ef4444'
      },
      {
        title: 'Monthly Revenue',
        value: `KSh ${(this.summary?.monthlyCollections ?? 0).toLocaleString()}`,
        trend: 12.4,
        previousValue: (this.summary?.monthlyCollections ?? 0) - 15000,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">trending_up</span>',
        color: '#059669'
      },
      {
        title: 'New Admissions',
        value: this.summary?.newAdmissions ?? 0,
        trend: 8.5,
        previousValue: (this.summary?.newAdmissions ?? 0) - 3,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">ads_click</span>',
        color: '#8b5cf6'
      },
      {
        title: 'Pending Approvals',
        value: 5,
        trend: -15.2,
        previousValue: 6,
        icon: '⏳',
        color: '#f59e0b'
      },
      {
        title: 'Online Users',
        value: 12,
        trend: 3.8,
        previousValue: 11,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">circle</span>',
        color: '#10b981'
      },
      {
        title: 'Database Status',
        value: 'Healthy',
        trend: 0,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">inventory</span>️',
        color: '#10b981'
      },
      {
        title: 'API Status',
        value: 'Running',
        trend: 0,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">bolt</span>',
        color: '#059669'
      },
      {
        title: 'Storage Used',
        value: '42%',
        trend: 2.1,
        previousValue: 40,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">save</span>',
        color: '#f59e0b'
      },
      {
        title: 'Server Health',
        value: 'Good',
        trend: 1.5,
        icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">desktop_windows</span>️',
        color: '#059669'
      }
    ];
    return this.metricCards;
  }

  loadDashboardSummary(): void {
    this.isLoadingSummary = true;
    this.dashboardService.getSummary().subscribe({
      next: (data) => {
        this.summary = data;
        this.notificationCount = data.notifications;
        this.metricCards = this.buildMetricCards();
        this.isLoadingSummary = false;
      },
      error: (err) => {
        console.error('Failed to load dashboard summary', err);
        this.isLoadingSummary = false;
      }
    });
  }

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
  }

  navigateToAnalytics(): void {
    this.router.navigate(['/analytics']);
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

  trackByTitle(index: number, item: MetricCard): any {
    return item.title;
  }

  navigateToMetric(card: MetricCard): void {
    if (card.action) {
      this.navigateTo(`/${card.action}`);
    }
  }
}
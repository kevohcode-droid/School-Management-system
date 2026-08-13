import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { DashboardService } from '../../services/dashboard.service';
import { Subscription } from 'rxjs';

interface ChartData {
  labels: string[];
  values: number[];
  colors: string[];
}

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './analytics.html',
  styleUrls: ['./analytics.css']
})
export class AnalyticsComponent implements OnInit, OnDestroy {
  currentUser: any = null;
  isLoading = true;
  private subscription: Subscription = new Subscription();

  activeTab: 'student' | 'attendance' | 'finance' | 'academic' = 'student';
  activeDateRange: string = 'Last 30 Days';
  activeAcademicYear: string = '2026/2027';
  activeTerm: string = 'Term 1';

  chartData: { [key: string]: ChartData } = {
    enrollment: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      values: [30, 45, 65, 80, 95, 100],
      colors: ['#3b82f6', '#60a5fa', '#93c5fd', '#bfdbfe', '#dbeafe', '#e0e7ff']
    },
    revenue: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      values: [120000, 150000, 180000, 220000, 280000, 350000],
      colors: ['#10b981', '#34d399', '#6ee7b7', '#a7f3d0', '#bbf7d0', '#bbf7d0']
    },
    attendance: {
      labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
      values: [92, 94, 89, 96, 91, 88, 93],
      colors: ['#3b82f6', '#60a5fa', '#93c5fd', '#bfdbfe', '#dbeafe', '#e0e7ff', '#dbeafe']
    },
    gender: {
      labels: ['Male', 'Female'],
      values: [52, 48],
      colors: ['#3b82f6', '#f472b6']
    },
    studentsPerClass: {
      labels: ['1A', '2A', '3A', '4A', '5A'],
      values: [45, 38, 42, 35, 40],
      colors: ['#8b5cf6', '#a78bfa', '#c4b5fd', '#ddd6fe', '#e0e7ff']
    },
    feeCollection: {
      labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
      values: [92, 94, 89, 96, 91, 88, 93],
      colors: ['#10b981', '#34d399', '#6ee7b7', '#a7f3d0', '#bbf7d0', '#bbf7d0', '#bbf7d0']
    }
  };

  constructor(
    public authService: AuthService,
    private router: Router,
    private dashboardService: DashboardService
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
      next: (user) => {
        this.currentUser = user;
        this.authService.saveCurrentUser(user);
      },
      error: () => {
        if (!this.currentUser) {
          this.router.navigate(['/login']);
        }
      }
    });

    this.loadAnalyticsData();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  loadAnalyticsData(): void {
    this.isLoading = true;
    this.dashboardService.getSummary().subscribe({
      next: (data) => {
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load analytics data', err);
        this.isLoading = false;
      }
    });
  }

  setActiveTab(tab: 'student' | 'attendance' | 'finance' | 'academic'): void {
    this.activeTab = tab;
  }

  setDateRange(range: string): void {
    this.activeDateRange = range;
  }

  setAcademicYear(year: string): void {
    this.activeAcademicYear = year;
  }

  setTerm(term: string): void {
    this.activeTerm = term;
  }

  hasRole(role: string | string[]): boolean {
    if (Array.isArray(role)) {
      return this.authService.hasAnyRole(role);
    }
    return this.authService.hasRole(role);
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  exportToExcel(section: string): void {
    alert(`Exporting ${section} to Excel...`);
  }

  exportToPDF(section: string): void {
    alert(`Exporting ${section} to PDF...`);
  }

  printReport(section: string): void {
    alert(`Printing ${section} report...`);
  }

  toggleFullScreen(): void {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen();
    } else {
      document.exitFullscreen();
    }
  }
}
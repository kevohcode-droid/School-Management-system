import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { StudentPortalService } from '../../../services/student-portal.service';
import { AuthService } from '../../../services/auth.service';
import { StudentDashboard } from '../../../models/student-portal';
import { CurrentUser } from '../../../models/auth';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './student-dashboard.html',
  styleUrls: ['./student-dashboard.css']
})
export class StudentDashboardComponent implements OnInit {
  dashboard: StudentDashboard | null = null;
  currentUser: CurrentUser | null = null;
  isLoading = false;
  errorMsg = '';

  constructor(
    private studentPortalService: StudentPortalService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUserFromStorage();
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.errorMsg = '';

    this.studentPortalService.getDashboard().subscribe({
      next: (data: StudentDashboard) => {
        this.dashboard = data;
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMsg = err?.error?.message ?? 'Failed to load dashboard. Please try again.';
        this.isLoading = false;
      }
    });
  }
}

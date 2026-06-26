import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
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

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    this.currentUser = this.authService.getCurrentUserFromStorage();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
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
}

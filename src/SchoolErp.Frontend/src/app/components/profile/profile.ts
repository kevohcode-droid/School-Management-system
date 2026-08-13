import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserProfile, UpdateProfileRequest } from '../../models/auth';

interface Activity {
  id: string;
  action: string;
  module: string;
  date: string;
  details: string;
}

interface EditProfileDialog {
  isOpen: boolean;
  fullName: string;
  phone: string;
  address: string;
  isSaving: boolean;
  error: string;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrls: ['./profile.css']
})
export class ProfileComponent implements OnInit {
  profile: UserProfile | null = null;
  isLoading = true;
  errorMessage = '';
  showActivityHistory = false;
  activities: Activity[] = [];

  editDialog: EditProfileDialog = {
    isOpen: false,
    fullName: '',
    phone: '',
    address: '',
    isSaving: false,
    error: ''
  };

  constructor(
    public authService: AuthService,
    private router: Router
  ) {
    this.activities = [
      { id: '1', action: 'LOGIN', module: 'Authentication', date: '2026-07-06T10:30:00Z', details: 'Successful login from Chrome on Windows' },
      { id: '2', action: 'VIEW_DASHBOARD', module: 'Dashboard', date: '2026-07-06T10:31:00Z', details: 'Viewed dashboard summary' },
      { id: '3', action: 'VIEW_STUDENTS', module: 'Students', date: '2026-07-06T10:45:00Z', details: 'Viewed students directory' },
      { id: '4', action: 'UPDATE_PROFILE', module: 'User Management', date: '2026-07-05T15:20:00Z', details: 'Updated profile information' },
      { id: '5', action: 'LOGIN', module: 'Authentication', date: '2026-07-04T09:00:00Z', details: 'Successful login from Chrome on Windows' }
    ];
  }

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.getUserProfile().subscribe({
      next: (data: any) => {
        this.profile = {
          fullName: data.fullName,
          email: data.email,
          phone: data.phone,
          role: data.role,
          school: data.school,
          tenantCode: data.tenantCode,
          username: data.username,
          lastLogin: data.lastLogin,
          dateJoined: data.dateJoined,
          address: data.address
        };
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading profile:', err);
        this.errorMessage = err.message || 'Failed to load profile.';
        this.isLoading = false;
      }
    });
  }

  get formattedLastLogin(): string {
    if (!this.profile?.lastLogin) return 'Never';
    const date = new Date(this.profile.lastLogin);
    return date.toLocaleString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  get accountStatus(): string {
    return 'Active';
  }

  get formattedDateJoined(): string {
    if (!this.profile?.dateJoined) return 'Unknown';
    return new Date(this.profile.dateJoined).toLocaleDateString();
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  changePassword(): void {
    this.router.navigate(['/change-password']);
  }

  openEditProfile(): void {
    if (!this.profile) return;

    this.editDialog = {
      isOpen: true,
      fullName: this.profile.fullName,
      phone: this.profile.phone || '',
      address: this.profile.address || '',
      isSaving: false,
      error: ''
    };
  }

  closeEditProfile(): void {
    this.editDialog.isOpen = false;
    this.editDialog.error = '';
  }

  saveProfile(): void {
    if (!this.profile) return;

    this.editDialog.isSaving = true;
    this.editDialog.error = '';

    const request: UpdateProfileRequest = {
      fullName: this.editDialog.fullName,
      phone: this.editDialog.phone || undefined,
      address: this.editDialog.address || undefined
    };

    this.authService.updateProfile(request).subscribe({
      next: () => {
        this.editDialog.isSaving = false;
        this.closeEditProfile();
        this.loadProfile();
      },
      error: (err) => {
        this.editDialog.error = err.message || 'Failed to update profile.';
        this.editDialog.isSaving = false;
      }
    });
  }

  toggleActivityHistory(): void {
    this.showActivityHistory = !this.showActivityHistory;
  }

  hasRole(role: string): boolean {
    return this.authService.hasRole(role);
  }
}
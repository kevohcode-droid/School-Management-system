import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { PreferencesService } from '../../services/preferences.service';

interface PreferencesForm {
  appearance: string;
  language: string;
  timezone: string;
  emailNotifications: boolean;
  smsNotifications: boolean;
  pushNotifications: boolean;
  compactMode: boolean;
  showWelcomeBanner: boolean;
  defaultLandingPage: string;
  autoLogoutMinutes: number;
}

@Component({
  selector: 'app-preferences',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './preferences.html',
  styleUrls: ['./preferences.css']
})
export class PreferencesComponent implements OnInit {
  form: PreferencesForm = {
    appearance: 'light',
    language: 'en',
    timezone: 'Africa/Nairobi',
    emailNotifications: true,
    smsNotifications: false,
    pushNotifications: true,
    compactMode: false,
    showWelcomeBanner: true,
    defaultLandingPage: '/dashboard',
    autoLogoutMinutes: 15
  };

  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  appearanceOptions = [
    { value: 'light', label: 'Light' },
    { value: 'dark', label: 'Dark' },
    { value: 'system', label: 'System' }
  ];

  languageOptions = [
    { value: 'en', label: 'English' },
    { value: 'sw', label: 'Swahili' }
  ];

  timezoneOptions = [
    { value: 'Africa/Nairobi', label: 'Africa/Nairobi (GMT+3)' },
    { value: 'UTC', label: 'UTC (GMT)' },
    { value: 'America/New_York', label: 'America/New York (GMT-5)' },
    { value: 'Europe/London', label: 'Europe/London (GMT+0)' }
  ];

  autoLogoutOptions = [
    { value: 15, label: '15 minutes' },
    { value: 30, label: '30 minutes' },
    { value: 60, label: '60 minutes' },
    { value: 0, label: 'Never' }
  ];

  constructor(
    public authService: AuthService,
    private preferencesService: PreferencesService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadPreferences();
  }

  loadPreferences(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.preferencesService.getPreferences().subscribe({
      next: (preferences) => {
        if (preferences) {
          this.form = { ...this.form, ...preferences };
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading preferences:', err);
        this.errorMessage = err.message || 'Failed to load preferences.';
        this.isLoading = false;
      }
    });
  }

  savePreferences(): void {
    this.isSaving = true;
    this.errorMessage = '';

    this.preferencesService.updatePreferences(this.form).subscribe({
      next: () => {
        this.successMessage = 'Preferences saved successfully.';
        this.isSaving = false;
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (err) => {
        this.errorMessage = err.message || 'Failed to save preferences.';
        this.isSaving = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  hasRole(role: string): boolean {
    return this.authService.hasRole(role);
  }
}
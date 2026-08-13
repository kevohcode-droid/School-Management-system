import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface PasswordForm {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

interface PasswordValidation {
  minLength: boolean;
  hasUpper: boolean;
  hasLower: boolean;
  hasNumber: boolean;
  hasSpecial: boolean;
  passwordsMatch: boolean;
}

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './change-password.html',
  styleUrls: ['./change-password.css']
})
export class ChangePasswordComponent {
  form: PasswordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  validation: PasswordValidation = {
    minLength: false,
    hasUpper: false,
    hasLower: false,
    hasNumber: false,
    hasSpecial: false,
    passwordsMatch: false
  };

  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  validatePassword(password: string): void {
    this.validation.minLength = password.length >= 8;
    this.validation.hasUpper = /[A-Z]/.test(password);
    this.validation.hasLower = /[a-z]/.test(password);
    this.validation.hasNumber = /\d/.test(password);
    this.validation.hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(password);
  }

  checkPasswordsMatch(): void {
    this.validation.passwordsMatch = this.form.newPassword === this.form.confirmPassword && this.form.confirmPassword.length > 0;
  }

  get passwordValid(): boolean {
    return this.validation.minLength &&
           this.validation.hasUpper &&
           this.validation.hasLower &&
           this.validation.hasNumber &&
           this.validation.hasSpecial &&
           this.validation.passwordsMatch;
  }

  onNewPasswordChange(): void {
    this.validatePassword(this.form.newPassword);
    this.checkPasswordsMatch();
  }

  onConfirmPasswordChange(): void {
    this.checkPasswordsMatch();
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.form.currentPassword) {
      this.errorMessage = 'Current password is required.';
      return;
    }

    if (!this.passwordValid) {
      this.errorMessage = 'New password does not meet the requirements.';
      return;
    }

    this.isSubmitting = true;

    this.authService.changePassword(this.form.currentPassword, this.form.newPassword).subscribe({
      next: () => {
        this.successMessage = 'Password updated successfully.';
        this.isSubmitting = false;
        setTimeout(() => {
          this.router.navigate(['/profile']);
        }, 1500);
      },
      error: (err) => {
        this.errorMessage = err.message || 'Failed to update password. Please check your current password.';
        this.isSubmitting = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/profile']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
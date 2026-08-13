import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface ResetForm {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reset-password.html',
  styleUrls: ['./reset-password.css']
})
export class ResetPasswordComponent {
  form: ResetForm = { email: '', token: '', newPassword: '', confirmPassword: '' };
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private route: ActivatedRoute,
    private authService: AuthService,
    private router: Router
  ) {
    this.route.queryParams.subscribe(params => {
      if (params['token']) this.form.token = params['token'];
      if (params['email']) this.form.email = params['email'];
    });
  }

  get passwordsMatch(): boolean {
    return this.form.newPassword === this.form.confirmPassword && this.form.newPassword.length > 0;
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.form.email || !this.form.token) {
      this.errorMessage = 'Invalid reset link.';
      return;
    }

    if (!this.passwordsMatch) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isSubmitting = true;
    this.authService.resetPassword(this.form.email, this.form.token, this.form.newPassword).subscribe({
      next: () => {
        this.successMessage = 'Password reset successful. Redirecting to login...';
        this.isSubmitting = false;
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err) => {
        this.errorMessage = err?.message || 'Failed to reset password.';
        this.isSubmitting = false;
      }
    });
  }
}

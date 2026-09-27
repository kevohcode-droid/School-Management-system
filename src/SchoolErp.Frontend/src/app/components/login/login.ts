import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { DashboardService } from '../../services/dashboard.service';
import { LoginRequest, RegisterRequest, AuthResponse, CurrentUser } from '../../models/auth';
import { DashboardPublic } from '../../models/dashboard';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent implements OnInit {
  isRegisterMode: boolean = false;
  errorMessage: string = '';
  isLoading: boolean = false;
  showPassword: boolean = false;
  showRegisterPassword: boolean = false;
  rememberMe: boolean = false;

  schoolName = 'Maina Group of Schools ERP';
  schoolMotto = 'Empowering Education Through Technology';
  todayDate = this.getFormattedDate();

  publicData: DashboardPublic | null = null;
  isLoadingPublicData = true;

  roleOptions = [
    { key: 'Student', label: 'Student', icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">school</span>' },
    { key: 'Parent', label: 'Parent', icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">group</span>' }
  ];
  selectedRole = 'Student';

  loginForm!: FormGroup;
  registerForm!: FormGroup;

  constructor(
    private authService: AuthService,
    private router: Router,
    private fb: FormBuilder,
    private dashboardService: DashboardService
  ) {
    this.initForms();
  }

  ngOnInit(): void {
    this.loadPublicData();
  }

  private initForms(): void {
    this.loginForm = this.fb.group({
      tenantCode: ['100', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });

    this.registerForm = this.fb.group({
      tenantCode: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), 
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/)]],
      confirmPassword: ['', [Validators.required]],
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      role: ['Student', [Validators.required]]
    }, { validator: this.passwordMatchValidator });
  }

  private passwordMatchValidator(formGroup: FormGroup): void {
    const password = formGroup.get('password')?.value;
    const confirmPassword = formGroup.get('confirmPassword')?.value;
    if (password !== confirmPassword) {
      formGroup.get('confirmPassword')?.setErrors({ passwordMismatch: true });
    }
  }

  private loadPublicData(): void {
    this.isLoadingPublicData = true;
    this.dashboardService.getPublicSummary().subscribe({
      next: (data) => {
        this.publicData = data;
        this.isLoadingPublicData = false;
      },
      error: (err) => {
        console.error('Failed to load public data', err);
        this.publicData = null;
        this.isLoadingPublicData = false;
      }
    });
  }

  toggleMode(): void {
    this.isRegisterMode = !this.isRegisterMode;
    this.errorMessage = '';
    if (this.isRegisterMode) {
      this.registerForm.reset({ role: 'Student' });
    } else {
      this.loginForm.reset({ tenantCode: '100', email: '', password: '', rememberMe: false });
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleRegisterPasswordVisibility(): void {
    this.showRegisterPassword = !this.showRegisterPassword;
  }

  selectLoginRole(role: string): void {
    this.selectedRole = role;
    this.errorMessage = '';
  }

  getSelectedRoleDescription(): string {
    const descriptions: Record<string, string> = {
      Admin: 'Manage school operations, users, academics and finances.',
      Teacher: 'Manage teaching, attendance and student academic progress.',
      Accountant: 'Manage school fees, payments, invoices, receipts and financial reports.',
      Staff: 'Access assigned administrative and school operational functions.',
      Student: 'View your personal academic information and school activities.',
      Parent: "View your child's academic, attendance and financial information."
    };
    return descriptions[this.selectedRole] || '';
  }

  private selectedRoleMatches(roles: string[]): boolean {
    return this.selectedRole === 'Admin'
      ? roles.includes('Admin') || roles.includes('SuperAdmin')
      : roles.includes(this.selectedRole);
  }

  private getFormattedDate(): string {
    const now = new Date();
    return now.toLocaleDateString('en-US', {
      weekday: 'long',
      month: 'long',
      day: 'numeric',
      year: 'numeric'
    });
  }

  onForgotPassword(): void {
    const email = this.loginForm.get('email')?.value;
    if (!email) {
      alert("Please enter your email address first.");
      return;
    }

    this.authService.forgotPassword(email).subscribe({
      next: (response) => {
        alert(response.message);
      },
      error: (err) => {
        console.error(err);
        alert("An error occurred. Please try again later.");
      }
    });
  }

  onSocialLogin(provider: string): void {
    alert("Social login is currently disabled. Please use email/password login.");
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const loginRequest: LoginRequest = {
      tenantCode: this.loginForm.value.tenantCode,
      email: this.loginForm.value.email,
      password: this.loginForm.value.password
    };

    this.authService.login(loginRequest).subscribe({
      next: (response: AuthResponse) => {
        if (!this.selectedRoleMatches(response.roles || [])) {
          this.authService.logout();
          this.errorMessage = `This account does not have ${this.selectedRole} access. Select the role assigned to your account.`;
          this.isLoading = false;
          return;
        }

        this.authService.saveToken(response.accessToken);
        this.authService.saveCurrentUser({
          userId: response.userId,
          userName: response.email,
          fullName: response.fullName,
          tenantId: response.tenantId,
          roles: response.roles,
          linkedStudentIds: response.linkedStudentIds || []
        });

        if (response.mustChangePassword) {
          localStorage.setItem('mustChangePassword', 'true');
          this.isLoading = false;
          this.router.navigate(['/change-password']);
          return;
        }
        localStorage.removeItem('mustChangePassword');
        
        if (this.loginForm.value.rememberMe) {
          localStorage.setItem('rememberMe', 'true');
        } else {
          localStorage.removeItem('rememberMe');
        }
        
        this.authService.getCurrentUser().subscribe({
          next: (user: CurrentUser) => {
            this.authService.saveCurrentUser(user);
            this.isLoading = false;
            this.router.navigate([user.roles?.includes('Student') ? '/student' : '/dashboard']);
          },
          error: (err: any) => {
            console.error('Error fetching current user:', err);
            this.isLoading = false;
            this.router.navigate([response.roles?.includes('Student') ? '/student' : '/dashboard']);
          }
        });
      },
      error: (error: any) => {
        console.error('Login error:', error);
        this.errorMessage = error.error?.errors?.[0] || error.message || 'Login failed. Please check your credentials.';
        this.isLoading = false;
      },
      complete: () => {
        console.log('Login observable completed');
      }
    });
  }

  onRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const registerRequest: RegisterRequest = {
      tenantCode: this.registerForm.value.tenantCode,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
      firstName: this.registerForm.value.firstName,
      lastName: this.registerForm.value.lastName,
      role: this.registerForm.value.role
    };

    this.authService.register(registerRequest).subscribe({
      next: (response: AuthResponse) => {
        this.authService.saveToken(response.accessToken);
        this.authService.saveCurrentUser({
          userId: response.userId,
          userName: response.email,
          fullName: response.fullName,
          tenantId: response.tenantId,
          roles: response.roles,
          linkedStudentIds: response.linkedStudentIds || []
        });
        
        this.authService.getCurrentUser().subscribe({
          next: (user: CurrentUser) => {
            this.authService.saveCurrentUser(user);
            this.isLoading = false;
            this.router.navigate(['/dashboard']);
          },
          error: (err: any) => {
            console.error('Error fetching current user after registration:', err);
            this.isLoading = false;
            this.router.navigate(['/dashboard']);
          }
        });
      },
      error: (error: any) => {
        console.error('Registration error:', error);
        this.errorMessage = error.error?.errors?.[0] || error.message || 'Registration failed. Check password criteria (uppercase, number, special char).';
        this.isLoading = false;
      },
      complete: () => {
        console.log('Registration observable completed');
      }
    });
  }

  get loginControls() {
    return this.loginForm.controls;
  }

  get registerControls() {
    return this.registerForm.controls;
  }
}

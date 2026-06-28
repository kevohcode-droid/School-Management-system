import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest, RegisterRequest, AuthResponse, CurrentUser } from '../../models/auth';
import { SocialAuthService, GoogleLoginProvider } from '@abacritt/angularx-social-login';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  isRegisterMode: boolean = false;
  errorMessage: string = '';
  isLoading: boolean = false;
  showPassword: boolean = false;
  showRegisterPassword: boolean = false;
  rememberMe: boolean = false;

  loginForm!: FormGroup;
  registerForm!: FormGroup;

  constructor(
    private authService: AuthService,
    private socialAuthService: SocialAuthService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.initForms();
  }

  private initForms(): void {
    this.loginForm = this.fb.group({
      tenantCode: ['', [Validators.required, Validators.minLength(2)]],
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
      role: ['Teacher', [Validators.required]]
    }, { validator: this.passwordMatchValidator });
  }

  private passwordMatchValidator(formGroup: FormGroup): void {
    const password = formGroup.get('password')?.value;
    const confirmPassword = formGroup.get('confirmPassword')?.value;
    if (password !== confirmPassword) {
      formGroup.get('confirmPassword')?.setErrors({ passwordMismatch: true });
    }
  }

  toggleMode(): void {
    this.isRegisterMode = !this.isRegisterMode;
    this.errorMessage = '';
    if (this.isRegisterMode) {
      this.registerForm.reset({ role: 'Teacher' });
    } else {
      this.loginForm.reset({ rememberMe: false });
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleRegisterPasswordVisibility(): void {
    this.showRegisterPassword = !this.showRegisterPassword;
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
    const tenantCode = this.loginForm.get('tenantCode')?.value;
    
    if (!tenantCode) {
      alert("Please enter a School / Tenant Code first!");
      return;
    }

    if (provider === 'google') {
      this.loginWithGoogle(tenantCode);
    }
  }

loginWithGoogle(tenantCode: string): void {
    this.socialAuthService.signIn(GoogleLoginProvider.PROVIDER_ID).then((user) => {
      if (!user.idToken) {
        this.errorMessage = 'Google authentication failed: no token received.';
        return;
      }
      this.authService.googleLogin({
        token: user.idToken,
        tenantCode: tenantCode
      }).subscribe({
        next: (response: AuthResponse) => {
          console.log('Google login successful:', response);
          this.authService.saveToken(response.accessToken);
          this.authService.saveCurrentUser({
            userId: response.userId,
            userName: response.email,
            tenantId: response.tenantId,
            roles: response.roles
          });
          this.router.navigate(['/dashboard']);
        },
        error: (err: any) => {
          console.error('Google login error:', err);
          this.errorMessage = err.error?.errors?.[0] || err.message || 'Google login failed.';
        }
      });
    }).catch((err) => {
      console.error('Google sign-in error:', err);
      this.errorMessage = 'Google sign-in was cancelled or failed.';
    });
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

    console.log('Login attempt:', loginRequest);

    this.authService.login(loginRequest).subscribe({
      next: (response: AuthResponse) => {
        console.log('Login successful:', response);
        this.authService.saveToken(response.accessToken);
        this.authService.saveCurrentUser({
          userId: response.userId,
          userName: response.email,
          tenantId: response.tenantId,
          roles: response.roles
        });
        
        if (this.loginForm.value.rememberMe) {
          localStorage.setItem('rememberMe', 'true');
        } else {
          localStorage.removeItem('rememberMe');
        }
        
        console.log('Fetching current user...');
        this.authService.getCurrentUser().subscribe({
          next: (user: CurrentUser) => {
            console.log('Current user fetched:', user);
            this.authService.saveCurrentUser(user);
            this.isLoading = false;
            this.router.navigate(['/dashboard']);
          },
          error: (err: any) => {
            console.error('Error fetching current user:', err);
            this.isLoading = false;
            this.router.navigate(['/dashboard']);
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

    console.log('Registration attempt:', registerRequest);

    this.authService.register(registerRequest).subscribe({
      next: (response: AuthResponse) => {
        console.log('Registration successful:', response);
        this.authService.saveToken(response.accessToken);
        this.authService.saveCurrentUser({
          userId: response.userId,
          userName: response.email,
          tenantId: response.tenantId,
          roles: response.roles
        });
        
        console.log('Fetching current user after registration...');
        this.authService.getCurrentUser().subscribe({
          next: (user: CurrentUser) => {
            console.log('Current user fetched:', user);
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
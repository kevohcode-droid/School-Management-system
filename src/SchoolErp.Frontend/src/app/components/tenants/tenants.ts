import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TenantService } from '../../services/tenant.service';
import { AuthService } from '../../services/auth.service';
import { Tenant, CreateTenantRequest } from '../../models/tenant';

@Component({
  selector: 'app-tenants',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tenants.html',
  styleUrls: ['./tenants.css']
})
export class TenantsComponent implements OnInit {
  tenants: Tenant[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';

  // Modals state
  showAddModal: boolean = false;

  // Forms data
  newTenant: CreateTenantRequest = {
    name: '',
    code: '',
    contactEmail: ''
  };

  constructor(
    private tenantService: TenantService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Check if user has SuperAdmin role
    if (!this.authService.hasRole('SuperAdmin')) {
      this.router.navigate(['/dashboard']);
      return;
    }

    this.loadTenants();
  }

  loadTenants(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.tenantService.getTenants().subscribe({
      next: (data: Tenant[]) => {
        this.tenants = data;
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load tenants. Verify role permission claims.';
        this.isLoading = false;
      }
    });
  }

  openAddModal(): void {
    this.newTenant = {
      name: '',
      code: '',
      contactEmail: ''
    };
    this.showAddModal = true;
  }

  closeAddModal(): void {
    this.showAddModal = false;
  }

  submitAddTenant(): void {
    this.isLoading = true;
    this.tenantService.createTenant(this.newTenant).subscribe({
      next: () => {
        this.closeAddModal();
        this.loadTenants();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.errors?.[0] || 'Failed to register school tenant. Code must be unique.';
        this.isLoading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}

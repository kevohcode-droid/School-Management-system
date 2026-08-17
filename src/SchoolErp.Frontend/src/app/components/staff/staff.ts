import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { StaffService } from '../../services/staff.service';
import { CreateStaffRequest, Staff } from '../../models/staff';

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './staff.html',
  styleUrls: ['./staff.css']
})
export class StaffComponent implements OnInit {
  staffList: Staff[] = [];
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  submittedStaff?: Partial<CreateStaffRequest>;
  showAddModal: boolean = false;

  designations: string[] = [
    'Teacher',
    'Principal',
    'Vice Principal',
    'Accountant',
    'Lab Technician',
    'Librarian',
    'Counselor',
    'Administrative Officer',
    'Support Staff'
  ];

  departments: string[] = [
    'Sciences',
    'Humanities',
    'Mathematics',
    'Languages',
    'Technical',
    'Arts',
    'Physical Education',
    'Administration'
  ];

  employmentStatuses: string[] = [
    'Full-time',
    'Part-time',
    'Contract',
    'Internship',
    'Volunteer'
  ];

  newStaff: CreateStaffRequest = {
    employeeId: 'EMP-' + Math.floor(1000 + Math.random() * 9000),
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    gender: 1,
    nationalId: '',
    designation: '',
    department: '',
    dateOfJoining: new Date().toISOString().split('T')[0],
    employmentStatus: 'Full-time',
    qualifications: ''
  };

  constructor(
    public authService: AuthService,
    private router: Router,
    public staffService: StaffService
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadStaff();
  }

  private loadStaff(): void {
    this.staffService.getStaff().subscribe({
      next: (data) => { this.staffList = data; },
      error: (err) => {
        console.error('Failed to load staff', err);
        this.errorMessage = 'Failed to load staff list';
      }
    });
  }

  openAddModal(): void {
    this.newStaff = {
      employeeId: 'EMP-' + Math.floor(1000 + Math.random() * 9000),
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      gender: 1,
      nationalId: '',
      designation: '',
      department: '',
      dateOfJoining: new Date().toISOString().split('T')[0],
      employmentStatus: 'Full-time',
      qualifications: ''
    };
    this.errorMessage = '';
    this.successMessage = '';
    this.submittedStaff = undefined;
    this.showAddModal = true;
  }

  closeAddModal(): void {
    this.showAddModal = false;
    this.submittedStaff = undefined;
  }

  submitAddStaff(): void {
    this.submittedStaff = { ...this.newStaff };

    const hasRequiredFields = (
      this.newStaff.employeeId.trim() !== '' &&
      this.newStaff.firstName.trim() !== '' &&
      this.newStaff.lastName.trim() !== '' &&
      this.newStaff.phone.trim() !== '' &&
      this.newStaff.designation.trim() !== '' &&
      this.newStaff.department.trim() !== ''
    );

    if (!hasRequiredFields) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.staffService.createStaff(this.newStaff).subscribe({
      next: (created) => {
        this.staffList.unshift(created);
        this.successMessage = `${this.newStaff.firstName} ${this.newStaff.lastName} has been onboarded successfully.`;
        this.closeAddModal();
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMessage = err.error?.errors?.[0] || err.message || 'Failed to add staff member';
        this.isLoading = false;
      }
    });
  }

  getGenderLabel(val: any): string {
    switch (String(val)) {
      case '1': case 'Male': return 'Male';
      case '2': case 'Female': return 'Female';
      default: return 'Other';
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  importStaff(): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.xlsx,.xls,.csv';
    input.onchange = (e: any) => {
      const file = e.target.files[0];
      if (file) {
        this.staffService.importStaff(file).subscribe({
          next: (result: any) => {
            this.successMessage = `Imported ${result.imported || 0} staff successfully`;
          },
          error: (err: any) => { this.errorMessage = 'Import failed: ' + (err.message || err.error?.errors?.[0] || 'Unknown error'); }
        });
      }
    };
    input.click();
  }

  exportStaff(): void {
    this.staffService.exportStaff();
  }

  downloadTemplate(): void {
    this.staffService.downloadTemplate();
  }
}
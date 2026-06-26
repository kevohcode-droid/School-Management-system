import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
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
  showAddModal: boolean = false;

  // Dropdown options
  designations: string[] = ['Teacher', 'Principal', 'Vice Principal', 'Accountant', 'Lab Technician', 'Librarian', 'Counselor', 'Administrative Officer', 'Support Staff'];
  departments: string[] = ['Sciences', 'Humanities', 'Mathematics', 'Languages', 'Technical', 'Arts', 'Physical Education', 'Administration'];
  employmentStatuses: string[] = ['Full-time', 'Part-time', 'Contract', 'Internship', 'Volunteer'];

  newStaff: CreateStaffRequest = this.defaultStaff();

  private defaultStaff(): CreateStaffRequest {
    return {
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
  }

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
    }
  }

  openAddModal(): void {
    this.newStaff = this.defaultStaff();
    this.errorMessage = '';
    this.successMessage = '';
    this.showAddModal = true;
  }

  closeAddModal(): void {
    this.showAddModal = false;
  }

  submitAddStaff(): void {
    this.isLoading = true;
    this.errorMessage = '';
    // Stubbed — backend endpoint not yet implemented
    setTimeout(() => {
      this.staffList.unshift({
        id: crypto.randomUUID(),
        tenantId: '',
        ...this.newStaff,
        createdAtUtc: new Date().toISOString()
      } as Staff);
      this.isLoading = false;
      this.successMessage = `${this.newStaff.firstName} ${this.newStaff.lastName} has been onboarded successfully.`;
      this.closeAddModal();
    }, 800);
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
}

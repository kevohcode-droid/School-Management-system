import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StudentService } from '../../services/student.service';
import { AuthService } from '../../services/auth.service';
import { Student, CreateStudentRequest, UpdateStudentRequest } from '../../models/student';

@Component({
  selector: 'app-students',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './students.html',
  styleUrls: ['./students.css']
})
export class StudentsComponent implements OnInit {
  students: Student[] = [];
  filteredStudents: Student[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';
  searchQuery: string = '';

  // Modals state
  showAddModal: boolean = false;
  showEditModal: boolean = false;
  showDeleteModal: boolean = false;

  // Dropdown data
  academicYears: string[] = ['2026 - Term 1', '2026 - Term 2', '2026 - Term 3', '2025 - Term 1', '2025 - Term 2', '2025 - Term 3'];
  nationalities: string[] = [
    'Kenyan', 'Ugandan', 'Tanzanian', 'Rwandan', 'Ethiopian', 'Somali',
    'South Sudanese', 'Congolese', 'Nigerian', 'Ghanaian', 'South African',
    'American', 'British', 'Indian', 'Chinese', 'Other'
  ];
  guardianRelationships: string[] = ['Father', 'Mother', 'Uncle', 'Aunt', 'Grandfather', 'Grandmother', 'Elder Sibling', 'Guardian', 'Other'];
  sections: string[] = ['North', 'South', 'East', 'West', 'Stream A', 'Stream B', 'Stream C'];

  private todayDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  private defaultNewStudent(): CreateStudentRequest {
    return {
      admissionNumber: 'ADM-' + Math.floor(1000 + Math.random() * 9000),
      admissionDate: this.todayDate(),
      firstName: '',
      middleName: '',
      lastName: '',
      email: '',
      gender: 1,
      dateOfBirth: '',
      nationality: '',
      academicYear: this.academicYears[0],
      sectionId: null,
      guardianName: '',
      guardianRelationship: '',
      guardianPhone: '',
      guardianEmail: ''
    };
  }

  // Forms data
  newStudent: CreateStudentRequest = this.defaultNewStudent();

  editingStudentId: string = '';
  editStudent: UpdateStudentRequest = {
    firstName: '',
    middleName: '',
    lastName: '',
    email: '',
    gender: 1,
    dateOfBirth: '',
    nationality: '',
    sectionId: null,
    guardianName: '',
    guardianRelationship: '',
    guardianPhone: '',
    guardianEmail: ''
  };

  studentToDelete: Student | null = null;

  constructor(
    private studentService: StudentService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadStudents();
  }

  loadStudents(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.studentService.getStudents().subscribe({
      next: (data: Student[]) => {
        this.students = data;
        this.filterStudents();
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load students. Please verify your permissions and credentials.';
        this.isLoading = false;
      }
    });
  }

  filterStudents(): void {
    const query = this.searchQuery.toLowerCase().trim();
    if (!query) {
      this.filteredStudents = this.students;
    } else {
      this.filteredStudents = this.students.filter(s =>
        s.firstName.toLowerCase().includes(query) ||
        s.lastName.toLowerCase().includes(query) ||
        s.admissionNumber.toLowerCase().includes(query) ||
        (s.email && s.email.toLowerCase().includes(query))
      );
    }
  }

  openAddModal(): void {
    this.newStudent = this.defaultNewStudent();
    this.showAddModal = true;
  }

  closeAddModal(): void {
    this.showAddModal = false;
  }

  submitAddStudent(): void {
    this.isLoading = true;
    const request: CreateStudentRequest = {
      ...this.newStudent,
      gender: Number(this.newStudent.gender)
    };

    this.studentService.createStudent(request).subscribe({
      next: () => {
        this.closeAddModal();
        this.loadStudents();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.errors?.[0] || 'Failed to create student. Check duplicate Admission No.';
        this.isLoading = false;
      }
    });
  }

  openEditModal(student: Student): void {
    this.editingStudentId = student.id;
    let dob = '';
    if (student.dateOfBirth) {
      if (Array.isArray(student.dateOfBirth)) {
        const [year, month, day] = student.dateOfBirth as any;
        dob = `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
      } else {
        dob = new Date(student.dateOfBirth).toISOString().split('T')[0];
      }
    }

    this.editStudent = {
      firstName: student.firstName,
      middleName: '',
      lastName: student.lastName,
      email: student.email,
      gender: Number(student.gender),
      dateOfBirth: dob,
      nationality: '',
      sectionId: student.sectionId || null,
      guardianName: '',
      guardianRelationship: '',
      guardianPhone: '',
      guardianEmail: ''
    };
    this.showEditModal = true;
  }

  closeEditModal(): void {
    this.showEditModal = false;
  }

  submitEditStudent(): void {
    this.isLoading = true;
    const request: UpdateStudentRequest = {
      ...this.editStudent,
      gender: Number(this.editStudent.gender)
    };
    this.studentService.updateStudent(this.editingStudentId, request).subscribe({
      next: () => {
        this.closeEditModal();
        this.loadStudents();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.errors?.[0] || 'Failed to update student.';
        this.isLoading = false;
      }
    });
  }

  openDeleteModal(student: Student): void {
    this.studentToDelete = student;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.studentToDelete = null;
  }

  confirmDeleteStudent(): void {
    if (!this.studentToDelete) return;
    this.isLoading = true;
    this.studentService.deleteStudent(this.studentToDelete.id).subscribe({
      next: () => {
        this.closeDeleteModal();
        this.loadStudents();
      },
      error: (err: any) => {
        this.errorMessage = 'Failed to delete student.';
        this.isLoading = false;
      }
    });
  }

  getGenderLabel(genderVal: any): string {
    const val = String(genderVal);
    switch (val) {
      case '1': case 'Male': return 'Male';
      case '2': case 'Female': return 'Female';
      case '3': case 'Other': return 'Other';
      default: return 'Unspecified';
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}

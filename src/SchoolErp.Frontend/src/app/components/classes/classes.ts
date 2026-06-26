import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { SchoolClass, CreateClassRequest } from '../../models/class';

@Component({
  selector: 'app-classes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './classes.html',
  styleUrls: ['./classes.css']
})
export class ClassesComponent implements OnInit {
  classes: SchoolClass[] = [];
  isLoading: boolean = false;
  successMessage: string = '';
  showAddModal: boolean = false;

  // Subject catalogue
  allSubjects: string[] = [
    'Mathematics', 'English Language', 'Kiswahili', 'Biology', 'Chemistry',
    'Physics', 'History', 'Geography', 'Business Studies', 'Computer Science',
    'Agriculture', 'Art & Design', 'Music', 'Physical Education', 'French',
    'Religious Education', 'Home Science', 'Technical Drawing'
  ];

  // Staff list (stubbed until staff API is ready)
  staffOptions: { id: string; name: string }[] = [
    { id: 'stub-1', name: 'Jane Doe (Mathematics)' },
    { id: 'stub-2', name: 'John Smith (Sciences)' },
    { id: 'stub-3', name: 'Mary Kamau (Humanities)' }
  ];

  rooms: string[] = [
    'Room 1A', 'Room 1B', 'Room 2A', 'Room 2B', 'Room 3A', 'Room 3B',
    'Science Lab', 'Computer Lab', 'Main Hall', 'Art Studio', 'Library'
  ];

  newClass: CreateClassRequest = this.defaultClass();

  private defaultClass(): CreateClassRequest {
    return {
      name: '',
      code: '',
      capacity: 40,
      classTeacherId: null,
      room: null,
      subjects: []
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
    this.newClass = this.defaultClass();
    this.successMessage = '';
    this.showAddModal = true;
  }

  closeAddModal(): void {
    this.showAddModal = false;
  }

  isSubjectSelected(subject: string): boolean {
    return this.newClass.subjects.includes(subject);
  }

  toggleSubject(subject: string): void {
    const idx = this.newClass.subjects.indexOf(subject);
    if (idx > -1) {
      this.newClass.subjects.splice(idx, 1);
    } else {
      this.newClass.subjects.push(subject);
    }
  }

  submitAddClass(): void {
    this.isLoading = true;
    // Stubbed — backend endpoint not yet implemented
    setTimeout(() => {
      const teacher = this.staffOptions.find(s => s.id === this.newClass.classTeacherId);
      this.classes.unshift({
        id: crypto.randomUUID(),
        tenantId: '',
        name: this.newClass.name,
        code: this.newClass.code,
        capacity: this.newClass.capacity,
        classTeacherId: this.newClass.classTeacherId || undefined,
        classTeacherName: teacher?.name,
        room: this.newClass.room || undefined,
        subjects: [...this.newClass.subjects],
        createdAtUtc: new Date().toISOString()
      });
      this.isLoading = false;
      this.successMessage = `Class "${this.newClass.name}" has been created successfully.`;
      this.closeAddModal();
    }, 800);
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StudentPortalService } from '../../../services/student-portal.service';
import { StudentProfile } from '../../../models/student-portal';

@Component({
  selector: 'app-student-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './student-profile.html',
  styleUrls: ['./student-profile.css']
})
export class StudentProfileComponent implements OnInit {
  profile: StudentProfile | null = null;
  loading = true;
  error: string | null = null;

  constructor(private studentPortalService: StudentPortalService) {}

  ngOnInit(): void {
    this.studentPortalService.getProfile().subscribe({
      next: (data) => {
        this.profile = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load profile. Please try again later.';
        this.loading = false;
      }
    });
  }

  /** Returns the two-letter initials for the avatar circle. */
  getInitials(): string {
    if (!this.profile) return '??';
    const first = this.profile.firstName?.charAt(0).toUpperCase() ?? '';
    const last  = this.profile.lastName?.charAt(0).toUpperCase() ?? '';
    return first + last;
  }

  /** Formats an ISO date string to a readable local date, or returns '—' if null. */
  formatDate(value: string | null): string {
    if (!value) return '—';
    const d = new Date(value);
    return isNaN(d.getTime()) ? value : d.toLocaleDateString(undefined, {
      year: 'numeric', month: 'long', day: 'numeric'
    });
  }
}

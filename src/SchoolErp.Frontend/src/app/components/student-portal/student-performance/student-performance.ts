import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentPortalService } from '../../../services/student-portal.service';
import {
  StudentPerformanceResponse,
  TermReport
} from '../../../models/student-portal';

@Component({
  selector: 'app-student-performance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './student-performance.html',
  styleUrls: ['./student-performance.css']
})
export class StudentPerformanceComponent implements OnInit {
  performanceData: StudentPerformanceResponse | null = null;
  selectedTerm: string = '';
  isLoading = false;
  error: string | null = null;

  constructor(private studentPortalService: StudentPortalService) {}

  ngOnInit(): void {
    this.loadPerformance();
  }

  loadPerformance(): void {
    this.isLoading = true;
    this.error = null;

    this.studentPortalService.getPerformance().subscribe({
      next: (data) => {
        this.performanceData = data;
        if (data.grouped && data.grouped.length > 0) {
          this.selectedTerm = data.grouped[0].term;
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load performance data. Please try again.';
        this.isLoading = false;
        console.error('Performance load error:', err);
      }
    });
  }

  get currentTermReport(): TermReport | undefined {
    return this.performanceData?.grouped.find(g => g.term === this.selectedTerm);
  }

  /**
   * Returns a badge CSS class based on the percentage score.
   * >=80 <span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">arrow_forward</span> A (success), >=65 <span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">arrow_forward</span> B (primary), >=50 <span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">arrow_forward</span> C (warning),
   * >=35 <span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">arrow_forward</span> D (neutral), <35 <span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">arrow_forward</span> E (danger)
   */
  getGrade(percentage: number): { letter: string; badgeClass: string } {
    if (percentage >= 80) return { letter: 'A', badgeClass: 'badge-success' };
    if (percentage >= 65) return { letter: 'B', badgeClass: 'badge-primary' };
    if (percentage >= 50) return { letter: 'C', badgeClass: 'badge-warning' };
    if (percentage >= 35) return { letter: 'D', badgeClass: 'badge-neutral' };
    return { letter: 'E', badgeClass: 'badge-danger' };
  }

  /** Selects a term from the historical trends section. */
  selectTerm(term: string): void {
    this.selectedTerm = term;
  }

  /**
   * Returns a CSS class for the term card left-border accent
   * based on the term's average grade band.
   */
  getTermAccentClass(average: number): string {
    if (average >= 80) return 'term-card--success';
    if (average >= 65) return 'term-card--primary';
    if (average >= 50) return 'term-card--warning';
    if (average >= 35) return 'term-card--neutral';
    return 'term-card--danger';
  }
}

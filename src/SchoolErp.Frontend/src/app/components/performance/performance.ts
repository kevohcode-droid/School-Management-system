import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PerformanceService } from '../../services/performance.service';
import { SectionsService } from '../../services/sections.service';
import { StudentService } from '../../services/student.service';
import { AuthService } from '../../services/auth.service';
import { StudentMarkDto, SaveStudentMarkItemDto, StudentMarkEntryRow } from '../../models/performance';
import { Student } from '../../models/student';

@Component({
  selector: 'app-performance',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './performance.html',
  styleUrls: ['./performance.css']
})
export class PerformanceComponent implements OnInit {
  activeTab: 'enter' | 'history' = 'enter';

  // Enter Tab State
  classes: any[] = [];
  selectedClassId: string = '';
  selectedSubject: string = 'Mathematics';
  customSubject: string = '';
  selectedTerm: string = 'Term 1 - 2026';
  customTerm: string = '';
  maxScore: number = 100;
  searchFilter: string = '';

  studentRows: StudentMarkEntryRow[] = [];
  analyticsMarks: StudentMarkDto[] = [];
  isLoadingStudents: boolean = false;
  isSaving: boolean = false;
  isSavingStudentId: string | null = null;
  successMessage: string = '';
  errorMessage: string = '';

  subjectOptions: string[] = [
    'Mathematics',
    'English Language',
    'Science',
    'Social Studies',
    'Kiswahili',
    'Biology',
    'Chemistry',
    'Physics',
    'Computer Studies',
    'History',
    'Geography',
    'Business Studies',
    'CRE/IRE',
    'Art & Design',
    'Music',
    'Other (Specify)'
  ];

  termOptions: string[] = [
    'Term 1 - 2026',
    'Term 2 - 2026',
    'Term 3 - 2026',
    'Midterm 1 - 2026',
    'Midterm 2 - 2026',
    'Midterm 3 - 2026',
    'Final Exam - 2026',
    'Other (Specify)'
  ];

  // History Tab State
  allStudents: Student[] = [];
  historyClassFilter: string = '';
  selectedStudentId: string = '';
  selectedStudent: Student | null = null;
  studentMarksHistory: StudentMarkDto[] = [];
  isLoadingHistory: boolean = false;
  historyErrorMessage: string = '';
  historySearchQuery: string = '';
  historyStudentSearch: string = '';
  historySubjectFilter: string = '';
  historyYearFilter: string = '';
  historyTermFilter: string = '';
  editingMarkId: string | null = null;
  editingScore: number | null = null;
  isSavingMarkId: string | null = null;
  reviewComment: string = '';

  constructor(
    private performanceService: PerformanceService,
    private sectionsService: SectionsService,
    private studentService: StudentService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadClasses();
    this.loadAllStudents();
  }

  get effectiveSubject(): string {
    return this.selectedSubject === 'Other (Specify)'
      ? this.customSubject.trim()
      : this.selectedSubject;
  }

  get effectiveTerm(): string {
    return this.selectedTerm === 'Other (Specify)'
      ? this.customTerm.trim()
      : this.selectedTerm;
  }

  get canEditMarks(): boolean {
    return this.authService.hasAnyRole(['Teacher', 'Admin', 'SuperAdmin']);
  }

  get canReviewMarks(): boolean {
    return this.authService.hasAnyRole(['Admin', 'SuperAdmin']);
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  setTab(tab: 'enter' | 'history'): void {
    this.activeTab = tab;
    this.successMessage = '';
    this.errorMessage = '';
  }

  loadClasses(): void {
    this.sectionsService.getSections().subscribe({
      next: (data) => {
        this.classes = data || [];
        if (this.classes.length > 0 && !this.selectedClassId) {
          this.selectedClassId = this.classes[0].id;
          this.onClassOrCriteriaChange();
        }
      },
      error: (err) => {
        this.errorMessage = 'Failed to load classes: ' + (err.message || 'Unknown error');
      }
    });
  }

  loadAllStudents(): void {
    this.studentService.getStudents().subscribe({
      next: (data) => {
        this.allStudents = data || [];
      },
      error: (err) => {
        console.error('Failed to load students list:', err);
      }
    });
  }

  get filteredStudentsForHistory(): Student[] {
    const query = this.historyStudentSearch.trim().toLowerCase();
    return this.allStudents.filter(student =>
      (!this.historyClassFilter || student.sectionId === this.historyClassFilter) &&
      (!query || `${student.firstName} ${student.lastName}`.toLowerCase().includes(query) ||
        (student.admissionNumber || '').toLowerCase().includes(query))
    );
  }

  onClassOrCriteriaChange(preserveNotifications: boolean = false): void {
    if (!this.selectedClassId) {
      this.studentRows = [];
      return;
    }

    this.isLoadingStudents = true;
    if (!preserveNotifications) {
      this.errorMessage = '';
      this.successMessage = '';
    }

    // 1. Get class students from allStudents or fetch fresh
    this.studentService.getStudents().subscribe({
      next: (all) => {
        this.allStudents = all || [];
        const classStudents = this.allStudents.filter(s => s.sectionId === this.selectedClassId);

        // 2. Fetch existing marks for this class & term if available
        const currentTerm = this.effectiveTerm;
        const currentSubject = this.effectiveSubject;

        this.performanceService.getClassPerformance(this.selectedClassId).subscribe({
          next: (existingMarks) => {
            this.analyticsMarks = existingMarks || [];
            const marksByStudent = new Map<string, StudentMarkDto>();
            (existingMarks || []).forEach(m => {
              if (m.subject.toLowerCase() === currentSubject.toLowerCase() && m.examTerm === currentTerm) {
                marksByStudent.set(m.studentId, m);
              }
            });

            this.studentRows = classStudents.map(student => {
              const existing = marksByStudent.get(student.id);
              const score = existing ? existing.scoreObtained : null;
              const row: StudentMarkEntryRow = {
                markId: existing?.id,
                studentId: student.id,
                admissionNumber: student.admissionNumber || '-',
                studentName: `${student.firstName} ${student.lastName}`.trim(),
                scoreObtained: score,
                maxScore: existing ? existing.maxScore : this.maxScore,
                percentage: 0,
                gradeBadge: '-',
                statusBadge: 'pending',
                remarks: '',
                reviewStatus: existing?.reviewStatus || 'Pending',
                isModified: false
              };
              this.calculateRowBadge(row);
              return row;
            });

            this.isLoadingStudents = false;
          },
          error: () => {
            this.analyticsMarks = [];
            this.errorMessage = 'Could not load existing marks. You can still enter new marks.';
            // Fallback: list students with blank marks
            this.studentRows = classStudents.map(student => {
              const row: StudentMarkEntryRow = {
                studentId: student.id,
                admissionNumber: student.admissionNumber || '-',
                studentName: `${student.firstName} ${student.lastName}`.trim(),
                scoreObtained: null,
                maxScore: this.maxScore,
                percentage: 0,
                gradeBadge: '-',
                statusBadge: 'pending',
                remarks: '',
                reviewStatus: 'Submitted',
                isModified: false
              };
              return row;
            });
            this.isLoadingStudents = false;
          }
        });
      },
      error: (err) => {
        this.isLoadingStudents = false;
        this.errorMessage = 'Could not load students for class: ' + (err.message || 'Unknown error');
      }
    });
  }

  onScoreInput(row: StudentMarkEntryRow): void {
    row.isModified = true;
    row.validationError = row.scoreObtained !== null &&
      (row.scoreObtained < 0 || row.scoreObtained > this.maxScore)
      ? `Enter a score from 0 to ${this.maxScore}.`
      : undefined;
    this.calculateRowBadge(row);
  }

  calculateRowBadge(row: StudentMarkEntryRow): void {
    const max = this.maxScore > 0 ? this.maxScore : 100;
    row.maxScore = max;

    if (row.scoreObtained === null || row.scoreObtained === undefined || isNaN(row.scoreObtained)) {
      row.percentage = 0;
      row.gradeBadge = '-';
      row.statusBadge = 'pending';
      row.remarks = '';
      return;
    }

    const pct = Math.round((row.scoreObtained / max) * 1000) / 10;
    row.percentage = pct;

    if (pct >= 90) {
      row.gradeBadge = 'A';
      row.statusBadge = 'excellent';
      row.remarks = 'Excellent';
    } else if (pct >= 80) {
      row.gradeBadge = 'A-';
      row.statusBadge = 'excellent';
      row.remarks = 'Very good';
    } else if (pct >= 70) {
      row.gradeBadge = 'B+';
      row.statusBadge = 'good';
      row.remarks = 'Good';
    } else if (pct >= 60) {
      row.gradeBadge = 'B';
      row.statusBadge = 'average';
      row.remarks = 'Satisfactory';
    } else if (pct >= 50) {
      row.gradeBadge = 'C';
      row.statusBadge = 'pass';
      row.remarks = 'Fair';
    } else if (pct >= 40) {
      row.gradeBadge = 'D';
      row.statusBadge = 'fail';
      row.remarks = 'Needs improvement';
    } else {
      row.gradeBadge = 'E';
      row.statusBadge = 'fail';
      row.remarks = 'Needs improvement';
    }
  }

  onMaxScoreChange(): void {
    this.studentRows.forEach(row => {
      this.calculateRowBadge(row);
      row.validationError = row.scoreObtained !== null && row.scoreObtained > this.maxScore
        ? `Enter a score from 0 to ${this.maxScore}.`
        : undefined;
    });
  }

  get filteredStudentRows(): StudentMarkEntryRow[] {
    if (!this.searchFilter.trim()) {
      return this.studentRows;
    }
    const q = this.searchFilter.toLowerCase();
    return this.studentRows.filter(r =>
      r.studentName.toLowerCase().includes(q) ||
      r.admissionNumber.toLowerCase().includes(q)
    );
  }

  get enteredCount(): number {
    return this.studentRows.filter(r => r.scoreObtained !== null && !isNaN(r.scoreObtained)).length;
  }

  get unsavedCount(): number {
    return this.studentRows.filter(row => row.isModified).length;
  }

  get classAverage(): number {
    const valid = this.studentRows.filter(r => r.scoreObtained !== null && !isNaN(r.scoreObtained));
    if (valid.length === 0) return 0;
    const total = valid.reduce((sum, r) => sum + r.percentage, 0);
    return Math.round((total / valid.length) * 10) / 10;
  }

  get highestScore(): number {
    const valid = this.studentRows.filter(r => r.scoreObtained !== null && !isNaN(r.scoreObtained));
    if (valid.length === 0) return 0;
    return Math.max(...valid.map(r => r.scoreObtained as number));
  }

  quickFill(score: number): void {
    this.studentRows.forEach(r => {
      r.scoreObtained = score;
      r.isModified = true;
      this.calculateRowBadge(r);
    });
  }

  clearAll(): void {
    this.studentRows.forEach(r => {
      r.scoreObtained = null;
      r.isModified = true;
      this.calculateRowBadge(r);
    });
  }

  submitMarks(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const subject = this.effectiveSubject;
    const examTerm = this.effectiveTerm;

    if (!subject) {
      this.errorMessage = 'Please specify a valid Subject.';
      return;
    }

    if (!examTerm) {
      this.errorMessage = 'Please specify an Exam Term.';
      return;
    }

    const invalidRows = this.studentRows.filter(r => r.scoreObtained !== null &&
      (!Number.isFinite(Number(r.scoreObtained)) || r.scoreObtained < 0 || r.scoreObtained > this.maxScore));
    if (invalidRows.length > 0) {
      invalidRows.forEach(row => this.onScoreInput(row));
      this.errorMessage = `Correct the ${invalidRows.length} invalid mark(s) before saving.`;
      return;
    }

    const marksToSave: SaveStudentMarkItemDto[] = this.studentRows
      .filter(r => r.scoreObtained !== null && !isNaN(r.scoreObtained))
      .map(r => ({
        id: r.markId,
        studentId: r.studentId,
        subject: subject,
        examTerm: examTerm,
        scoreObtained: Number(r.scoreObtained),
        maxScore: this.maxScore > 0 ? this.maxScore : 100
      }));

    if (marksToSave.length === 0) {
      this.errorMessage = 'Please enter scores for at least one student before submitting.';
      return;
    }

    this.isSaving = true;

    this.performanceService.bulkSaveMarks(marksToSave).subscribe({
      next: (res) => {
        this.isSaving = false;
        this.successMessage = res?.message || `Successfully saved ${marksToSave.length} student mark(s)!`;
        this.studentRows.forEach(r => {
          if (r.scoreObtained !== null) {
            r.isModified = false;
            r.validationError = undefined;
            r.reviewStatus = 'Submitted';
          }
        });
        this.onClassOrCriteriaChange(true);
      },
      error: (err) => {
        this.isSaving = false;
        this.errorMessage = 'Failed to save marks: ' + (err.error?.message || err.message || 'Server error');
      }
    });
  }

  saveStudentMark(row: StudentMarkEntryRow): void {
    if (row.scoreObtained === null || !Number.isFinite(Number(row.scoreObtained)) ||
        row.scoreObtained < 0 || row.scoreObtained > this.maxScore) {
      this.onScoreInput(row);
      row.validationError = `Enter a score from 0 to ${this.maxScore} before saving.`;
      return;
    }

    const subject = this.effectiveSubject;
    const examTerm = this.effectiveTerm;
    if (!subject || !examTerm) {
      this.errorMessage = 'Choose a subject and exam term before saving.';
      return;
    }

    this.isSavingStudentId = row.studentId;
    this.errorMessage = '';
    this.successMessage = '';
    this.performanceService.bulkSaveMarks([{
      id: row.markId,
      studentId: row.studentId,
      subject,
      examTerm,
      scoreObtained: Number(row.scoreObtained),
      maxScore: this.maxScore
    }]).subscribe({
      next: result => {
        this.isSavingStudentId = null;
        row.isModified = false;
        row.reviewStatus = 'Submitted';
        row.validationError = undefined;
        this.successMessage = result?.message || 'Student mark saved.';
        this.onClassOrCriteriaChange(true);
      },
      error: error => {
        this.isSavingStudentId = null;
        this.errorMessage = 'Could not save mark: ' + (error.error?.message || error.message || 'Server error');
      }
    });
  }

  importCsv(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    file.text().then(text => {
      const lines = text.split(/\r?\n/).filter(line => line.trim());
      if (lines.length < 2) throw new Error('The file must include a header and at least one mark.');
      const headers = this.parseCsvLine(lines[0]).map(value => value.trim().toLowerCase());
      const admissionIndex = headers.findIndex(value => ['admission no', 'admission number', 'admissionnumber'].includes(value));
      const marksIndex = headers.findIndex(value => ['marks', 'score', 'score obtained'].includes(value));
      if (admissionIndex < 0 || marksIndex < 0) throw new Error('Include Admission No and Marks columns.');

      const rowsByAdmission = new Map(this.studentRows.map(row => [row.admissionNumber.toLowerCase(), row]));
      let imported = 0;
      for (const line of lines.slice(1)) {
        const values = this.parseCsvLine(line);
        const row = rowsByAdmission.get((values[admissionIndex] || '').trim().toLowerCase());
        const markValue = values[marksIndex]?.trim();
        const score = Number(markValue);
        if (!row || !markValue || !Number.isFinite(score)) continue;
        row.scoreObtained = score;
        this.onScoreInput(row);
        imported++;
      }
      this.successMessage = `Imported ${imported} mark(s). Review the rows, then save.`;
      this.errorMessage = imported ? '' : 'No matching students with valid marks were found in the file.';
    }).catch(error => {
      this.errorMessage = error.message || 'Could not read the selected file.';
    }).finally(() => {
      input.value = '';
    });
  }

  exportCsv(): void {
    const escape = (value: string | number) => `"${String(value).replace(/"/g, '""')}"`;
    const rows = [
      ['Admission No', 'Student Name', 'Marks', 'Maximum Score', 'Percentage', 'Grade', 'Remarks'],
      ...this.filteredStudentRows.map(row => [
        row.admissionNumber, row.studentName, row.scoreObtained ?? '', this.maxScore,
        row.scoreObtained === null ? '' : `${row.percentage}%`, row.gradeBadge, row.remarks
      ])
    ];
    const csv = rows.map(row => row.map(escape).join(',')).join('\r\n');
    const link = document.createElement('a');
    link.href = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }));
    link.download = `marks-${this.effectiveSubject || 'subject'}-${this.effectiveTerm || 'term'}.csv`;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  private parseCsvLine(line: string): string[] {
    const values: string[] = [];
    let value = '';
    let quoted = false;
    for (let index = 0; index < line.length; index++) {
      const character = line[index];
      if (character === '"' && line[index + 1] === '"' && quoted) {
        value += '"';
        index++;
      } else if (character === '"') {
        quoted = !quoted;
      } else if (character === ',' && !quoted) {
        values.push(value);
        value = '';
      } else {
        value += character;
      }
    }
    values.push(value);
    return values;
  }

  // History Tab Methods
  onStudentSelected(): void {
    this.studentMarksHistory = [];
    this.historyErrorMessage = '';
    this.editingMarkId = null;
    this.editingScore = null;

    if (!this.selectedStudentId) {
      this.selectedStudent = null;
      return;
    }

    this.selectedStudent = this.allStudents.find(s => s.id === this.selectedStudentId) || null;
    this.isLoadingHistory = true;

    this.performanceService.getStudentPerformance(this.selectedStudentId).subscribe({
      next: (marks) => {
        this.studentMarksHistory = marks || [];
        this.isLoadingHistory = false;
      },
      error: (err) => {
        this.isLoadingHistory = false;
        this.historyErrorMessage = 'Could not load student performance: ' + (err.error?.message || err.message || 'Server error');
      }
    });
  }

  get filteredHistoryMarks(): StudentMarkDto[] {
    const q = this.historySearchQuery.toLowerCase();
    return this.studentMarksHistory.filter(m =>
      (!q || m.subject.toLowerCase().includes(q) || m.examTerm.toLowerCase().includes(q)) &&
      (!this.historySubjectFilter || m.subject === this.historySubjectFilter) &&
      (!this.historyTermFilter || m.examTerm === this.historyTermFilter) &&
      (!this.historyYearFilter || (m.examTerm.match(/20\d{2}/)?.[0] || String(new Date(m.dateRecorded).getFullYear())) === this.historyYearFilter)
    );
  }

  editMark(mark: StudentMarkDto): void {
    this.editingMarkId = mark.id;
    this.editingScore = mark.scoreObtained;
    this.errorMessage = '';
    this.successMessage = '';
  }

  cancelMarkEdit(): void {
    this.editingMarkId = null;
    this.editingScore = null;
  }

  saveMark(mark: StudentMarkDto): void {
    const score = Number(this.editingScore);
    if (!Number.isFinite(score) || score < 0 || score > mark.maxScore) {
      this.errorMessage = `Enter a score from 0 to ${mark.maxScore}.`;
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.isSavingMarkId = mark.id;

    this.performanceService.bulkSaveMarks([{
      id: mark.id,
      studentId: mark.studentId,
      subject: mark.subject,
      examTerm: mark.examTerm,
      scoreObtained: score,
      maxScore: mark.maxScore,
      dateRecorded: mark.dateRecorded
    }]).subscribe({
      next: (result) => {
        this.isSavingMarkId = null;
        this.editingMarkId = null;
        this.editingScore = null;
        this.successMessage = result?.message || 'Mark updated successfully.';
        this.onStudentSelected();
      },
      error: (error) => {
        this.isSavingMarkId = null;
        this.errorMessage = 'Could not update mark: ' + (error.error?.message || error.message || 'Server error');
      }
    });
  }

  reviewMark(mark: StudentMarkDto, status: string): void {
    this.isSavingMarkId = mark.id;
    this.errorMessage = '';
    this.successMessage = '';
    this.performanceService.reviewMark(mark.id, status, this.reviewComment).subscribe({
      next: result => {
        this.isSavingMarkId = null;
        mark.reviewStatus = result.status;
        mark.reviewComment = this.reviewComment.trim() || null;
        this.reviewComment = '';
        this.successMessage = result.message;
      },
      error: error => {
        this.isSavingMarkId = null;
        this.errorMessage = 'Could not review mark: ' + (error.error?.message || error.message || 'Server error');
      }
    });
  }

  reviewStudentMark(row: StudentMarkEntryRow, status: string): void {
    if (!row.markId) return;
    this.isSavingStudentId = row.studentId;
    this.errorMessage = '';
    this.successMessage = '';
    this.performanceService.reviewMark(row.markId, status, row.reviewComment).subscribe({
      next: result => {
        this.isSavingStudentId = null;
        row.reviewStatus = result.status;
        this.successMessage = result.message;
      },
      error: error => {
        this.isSavingStudentId = null;
        this.errorMessage = 'Could not review mark: ' + (error.error?.message || error.message || 'Server error');
      }
    });
  }

  reopenStudentMark(row: StudentMarkEntryRow): void {
    if (!row.markId) return;
    this.isSavingStudentId = row.studentId;
    this.errorMessage = '';
    this.successMessage = '';
    this.performanceService.reopenMark(row.markId).subscribe({
      next: result => {
        this.isSavingStudentId = null;
        row.reviewStatus = result.status;
        row.reviewComment = 'Reopened by an administrator.';
        this.successMessage = result.message;
      },
      error: error => {
        this.isSavingStudentId = null;
        this.errorMessage = 'Could not reopen mark: ' + (error.error?.message || error.message || 'Server error');
      }
    });
  }

  reopenMark(mark: StudentMarkDto): void {
    this.isSavingMarkId = mark.id;
    this.errorMessage = '';
    this.successMessage = '';
    this.performanceService.reopenMark(mark.id).subscribe({
      next: result => {
        this.isSavingMarkId = null;
        mark.reviewStatus = result.status;
        mark.reviewComment = 'Reopened by an administrator.';
        this.successMessage = result.message;
      },
      error: error => {
        this.isSavingMarkId = null;
        this.errorMessage = 'Could not reopen mark: ' + (error.error?.message || error.message || 'Server error');
      }
    });
  }

  get historyTotalExams(): number {
    return this.studentMarksHistory.length;
  }

  get historyAveragePercentage(): number {
    if (this.studentMarksHistory.length === 0) return 0;
    const sum = this.studentMarksHistory.reduce((acc, m) => acc + m.percentage, 0);
    return Math.round((sum / this.studentMarksHistory.length) * 10) / 10;
  }

  get historyOverallGrade(): string {
    return this.gradeFor(this.historyAveragePercentage);
  }

  get historyHighestScore(): { score: number; subject: string; term: string } | null {
    if (this.studentMarksHistory.length === 0) return null;
    const highest = [...this.studentMarksHistory].sort((a, b) => b.percentage - a.percentage)[0];
    return { score: highest.percentage, subject: highest.subject, term: highest.examTerm };
  }

  get historyPassRate(): number {
    if (this.studentMarksHistory.length === 0) return 0;
    const passed = this.studentMarksHistory.filter(m => m.percentage >= 50).length;
    return Math.round((passed / this.studentMarksHistory.length) * 100);
  }

  get analyticsClassAverage(): number {
    if (!this.analyticsMarks.length) return 0;
    return Math.round(this.analyticsMarks.reduce((sum, mark) => sum + mark.percentage, 0) / this.analyticsMarks.length * 10) / 10;
  }

  get analyticsPassRate(): number {
    if (!this.analyticsMarks.length) return 0;
    return Math.round(this.analyticsMarks.filter(mark => mark.percentage >= 50).length / this.analyticsMarks.length * 100);
  }

  get studentAverages(): { studentId: string; name: string; average: number }[] {
    const grouped = new Map<string, StudentMarkDto[]>();
    this.analyticsMarks.forEach(mark => grouped.set(mark.studentId, [...(grouped.get(mark.studentId) || []), mark]));
    return [...grouped.entries()].map(([studentId, marks]) => ({
      studentId,
      name: marks[0].studentName,
      average: marks.reduce((sum, mark) => sum + mark.percentage, 0) / marks.length
    })).sort((a, b) => b.average - a.average);
  }

  get highestStudentAverage(): number {
    return this.studentAverages[0]?.average ?? 0;
  }

  get lowestStudentAverage(): number {
    return this.studentAverages.length ? this.studentAverages[this.studentAverages.length - 1].average : 0;
  }

  get subjectAverages(): { label: string; value: number }[] {
    return this.averageBy(this.analyticsMarks, mark => mark.subject);
  }

  get termAverages(): { label: string; value: number }[] {
    return this.averageBy(this.analyticsMarks, mark => mark.examTerm);
  }

  get gradeDistribution(): { label: string; value: number }[] {
    return ['A', 'A-', 'B+', 'B', 'C', 'D', 'E'].map(label => ({
      label,
      value: this.analyticsMarks.filter(mark => this.gradeFor(mark.percentage) === label).length
    }));
  }

  get academicAlerts(): { studentId: string; message: string }[] {
    const alerts: { studentId: string; message: string }[] = [];
    const currentMarks = new Set(this.analyticsMarks
      .filter(mark => mark.subject.toLowerCase() === this.effectiveSubject.toLowerCase() && mark.examTerm === this.effectiveTerm)
      .map(mark => mark.studentId));

    this.studentRows.forEach(row => {
      if (row.scoreObtained !== null && row.percentage < 50) {
        alerts.push({ studentId: row.studentId, message: `${row.studentName} — ${this.effectiveSubject} is below the 50% pass mark.` });
      } else if (row.scoreObtained === null && !currentMarks.has(row.studentId)) {
        alerts.push({ studentId: row.studentId, message: `${row.studentName} — Missing marks for ${this.effectiveSubject}.` });
      }
    });

    const grouped = new Map<string, StudentMarkDto[]>();
    this.analyticsMarks.forEach(mark => {
      const key = `${mark.studentId}:${mark.subject.toLowerCase()}`;
      grouped.set(key, [...(grouped.get(key) || []), mark]);
    });
    grouped.forEach(marks => {
      const chronological = [...marks].sort((a, b) => new Date(a.dateRecorded).getTime() - new Date(b.dateRecorded).getTime());
      if (chronological.length < 2) return;
      const previous = chronological[chronological.length - 2];
      const latest = chronological[chronological.length - 1];
      if (previous.percentage - latest.percentage >= 15) {
        alerts.push({ studentId: latest.studentId, message: `${latest.studentName} — ${latest.subject} dropped from ${previous.percentage}% to ${latest.percentage}%.` });
      }
    });
    return alerts;
  }

  private averageBy(marks: StudentMarkDto[], key: (mark: StudentMarkDto) => string): { label: string; value: number }[] {
    const grouped = new Map<string, number[]>();
    marks.forEach(mark => grouped.set(key(mark), [...(grouped.get(key(mark)) || []), mark.percentage]));
    return [...grouped.entries()].map(([label, values]) => ({
      label,
      value: Math.round(values.reduce((sum, value) => sum + value, 0) / values.length * 10) / 10
    }));
  }

  gradeFor(percentage: number): string {
    if (percentage >= 90) return 'A';
    if (percentage >= 80) return 'A-';
    if (percentage >= 70) return 'B+';
    if (percentage >= 60) return 'B';
    if (percentage >= 50) return 'C';
    if (percentage >= 40) return 'D';
    return 'E';
  }

  openStudentHistory(studentId: string): void {
    this.selectedStudentId = studentId;
    this.activeTab = 'history';
    this.onStudentSelected();
  }

  exportStudentHistory(): void {
    const escape = (value: string | number) => `"${String(value).replace(/"/g, '""')}"`;
    const rows = [
      ['Academic Year', 'Term', 'Subject', 'Marks', 'Maximum Score', 'Percentage', 'Grade'],
      ...this.filteredHistoryMarks.map(mark => [
        mark.examTerm.match(/20\d{2}/)?.[0] || new Date(mark.dateRecorded).getFullYear(),
        mark.examTerm, mark.subject, mark.scoreObtained, mark.maxScore, `${mark.percentage}%`, this.gradeFor(mark.percentage)
      ])
    ];
    const link = document.createElement('a');
    link.href = URL.createObjectURL(new Blob([rows.map(row => row.map(escape).join(',')).join('\r\n')], { type: 'text/csv;charset=utf-8' }));
    link.download = `performance-${this.selectedStudent?.admissionNumber || 'student'}.csv`;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  getAcademicYear(mark: StudentMarkDto): string {
    return mark.examTerm.match(/20\d{2}/)?.[0] || String(new Date(mark.dateRecorded).getFullYear());
  }

  printStudentHistory(): void {
    window.print();
  }

  get historySubjectOptions(): string[] {
    return [...new Set(this.studentMarksHistory.map(mark => mark.subject))].sort();
  }

  get historyTermOptions(): string[] {
    return [...new Set(this.studentMarksHistory.map(mark => mark.examTerm))].sort();
  }

  get historyYears(): string[] {
    return [...new Set(this.studentMarksHistory.map(mark =>
      mark.examTerm.match(/20\d{2}/)?.[0] || String(new Date(mark.dateRecorded).getFullYear())
    ))].sort().reverse();
  }

  get historySubjectCount(): number {
    return this.historySubjectOptions.length;
  }

  get historySubjectAverages(): { subject: string; average: number }[] {
    return this.averageBy(this.studentMarksHistory, mark => mark.subject)
      .map(item => ({ subject: item.label, average: item.value }));
  }

  get historyTimeline(): { label: string; subject: string; value: number }[] {
    return [...this.studentMarksHistory]
      .sort((a, b) => new Date(a.dateRecorded).getTime() - new Date(b.dateRecorded).getTime())
      .map(mark => ({ label: mark.examTerm, subject: mark.subject, value: mark.percentage }));
  }

  get bestHistorySubject(): string {
    return [...this.historySubjectAverages].sort((a, b) => b.average - a.average)[0]?.subject || '—';
  }

  get weakestHistorySubject(): string {
    return [...this.historySubjectAverages].sort((a, b) => a.average - b.average)[0]?.subject || '—';
  }

  getGradeBadgeText(pct: number): string {
    return this.gradeFor(pct);
  }

  getGradeBadgeClass(pct: number): string {
    if (pct >= 80) return 'badge-a';
    if (pct >= 70) return 'badge-b';
    if (pct >= 50) return 'badge-c';
    if (pct >= 40) return 'badge-d';
    return 'badge-e';
  }
}

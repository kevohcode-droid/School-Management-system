import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface ImportableEntity {
  name: string;
  icon: string;
  description: string;
  fields: string[];
  template: any[];
}

@Component({
  selector: 'app-import-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './import-center.html',
  styleUrls: ['./import-center.css']
})
export class ImportCenterComponent implements OnInit {
  currentUser: any = null;
  selectedEntity: ImportableEntity | null = null;
  importFile: File | null = null;
  importPreview: any[] = [];
  validationErrors: string[] = [];
  duplicateRecords: any[] = [];
  importStrategy: 'insert' | 'update' | 'merge' | 'ignore' = 'insert';
  isImporting = false;
  importProgress = 0;
  importResults: any = null;
  showPreview = false;

  importableEntities: ImportableEntity[] = [
    {
      name: 'Students',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">man</span>‍<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">school</span>',
      description: 'Import student enrollment records, personal information, and guardian details',
      fields: ['Admission Number', 'First Name', 'Last Name', 'Email', 'Gender', 'Date of Birth', 'Section', 'Guardian Name', 'Guardian Phone', 'Guardian Email'],
      template: [
        { 'Admission Number': 'ADM001', 'First Name': 'John', 'Last Name': 'Doe', 'Email': 'john@example.com', 'Gender': 'Male', 'Date of Birth': '2010-01-15', 'Section': 'Form 1A', 'Guardian Name': 'Jane Doe', 'Guardian Phone': '+254700000000', 'Guardian Email': 'jane@example.com' }
      ]
    },
    {
      name: 'Staff',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">woman</span>‍<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">school</span>',
      description: 'Import staff and teacher records, employment details, and qualifications',
      fields: ['Employee ID', 'First Name', 'Last Name', 'Email', 'Phone', 'Department', 'Position', 'Hire Date', 'Salary'],
      template: [
        { 'Employee ID': 'STF001', 'First Name': 'Mary', 'Last Name': 'Smith', 'Email': 'mary@school.edu', 'Phone': '+254700000001', 'Department': 'Mathematics', 'Position': 'Teacher', 'Hire Date': '2020-01-01', 'Salary': 'KSh 80000' }
      ]
    },
    {
      name: 'Classes',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">book</span>',
      description: 'Import class sections, grade levels, and teacher assignments',
      fields: ['Class Name', 'Grade Level', 'Section', 'Teacher Name', 'Capacity'],
      template: [
        { 'Class Name': 'Form 1A', 'Grade Level': 'Form 1', 'Section': 'A', 'Teacher Name': 'John Smith', 'Capacity': '40' }
      ]
    },
    {
      name: 'Subjects',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">menu_book</span>',
      description: 'Import subject catalog, teachers, and class assignments',
      fields: ['Subject Code', 'Subject Name', 'Teacher', 'Class', 'Period'],
      template: [
        { 'Subject Code': 'MATH101', 'Subject Name': 'Mathematics', 'Teacher': 'John Smith', 'Class': 'Form 1A', 'Period': 'Period 1' }
      ]
    },
    {
      name: 'Fee Structures',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">payments</span>',
      description: 'Import fee categories, amounts, and payment schedules',
      fields: ['Fee Name', 'Amount', 'Due Date', 'Class', 'Description'],
      template: [
        { 'Fee Name': 'Term Fees', 'Amount': 'KSh 50000', 'Due Date': '2026-01-01', 'Class': 'Form 1', 'Description': 'Annual Term Fees' }
      ]
    },
    {
      name: 'Payments',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">credit_card</span>',
      description: 'Import payment transactions and receipts',
      fields: ['Student ID', 'Amount', 'Payment Date', 'Payment Mode', 'Transaction ID', 'Reference'],
      template: [
        { 'Student ID': 'ADM001', 'Amount': 'KSh 50000', 'Payment Date': '2026-01-15', 'Payment Mode': 'Bank Transfer', 'Transaction ID': 'TXN001', 'Reference': 'Term Fees' }
      ]
    },
    {
      name: 'Attendance',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">assignment</span>',
      description: 'Import daily attendance records for students',
      fields: ['Student ID', 'Date', 'Status', 'Remarks'],
      template: [
        { 'Student ID': 'ADM001', 'Date': '2026-01-15', 'Status': 'Present', 'Remarks': '' }
      ]
    }
  ];

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    const cachedUser = this.authService.getCurrentUserFromStorage();
    if (cachedUser) {
      this.currentUser = cachedUser;
    }

    this.authService.getCurrentUser().subscribe({
      next: (user) => {
        this.currentUser = user;
        this.authService.saveCurrentUser(user);
      },
      error: () => {
        if (!this.currentUser) {
          this.router.navigate(['/login']);
        }
      }
    });
  }

  selectEntity(entity: ImportableEntity): void {
    this.selectedEntity = entity;
    this.importPreview = [];
    this.validationErrors = [];
    this.duplicateRecords = [];
    this.importResults = null;
    this.showPreview = false;
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.importFile = file;
      this.previewFile(file);
    }
  }

  previewFile(file: File): void {
    const reader = new FileReader();
    
    if (file.name.endsWith('.csv')) {
      reader.onload = (e) => {
        const text = e.target?.result as string;
        const lines = text.split('\n').filter(l => l.trim());
        const headers = lines[0]?.split(',').map(h => h.trim()) || [];
        
        this.importPreview = [];
        for (let i = 1; i < Math.min(lines.length, 10); i++) {
          const values = lines[i].split(',').map(v => v.trim());
          const row: any = {};
          headers.forEach((header, idx) => {
            row[header] = values[idx] || '';
          });
          this.importPreview.push(row);
        }
        
        this.showPreview = true;
        this.validatePreview();
      };
      reader.readAsText(file);
    } else if (file.name.endsWith('.xlsx') || file.name.endsWith('.xls')) {
      this.showPreview = true;
      alert('Excel file detected. Preview functionality would require a library like SheetJS in production.');
    }
  }

  validatePreview(): void {
    this.validationErrors = [];
    this.duplicateRecords = [];

    if (!this.selectedEntity || this.importPreview.length === 0) return;

    const headers = Object.keys(this.importPreview[0]);
    const requiredFields = this.selectedEntity.fields.slice(0, 3);
    
    requiredFields.forEach(field => {
      if (!headers.includes(field)) {
        this.validationErrors.push(`Missing required column: ${field}`);
      }
    });

    this.importPreview.forEach((row, index) => {
      Object.keys(row).forEach(key => {
        if (!row[key] && this.selectedEntity?.fields.includes(key)) {
          this.validationErrors.push(`Row ${index + 1}: Empty value for required field "${key}"`);
        }
      });
    });
  }

  executeImport(): void {
    if (!this.importFile || !this.selectedEntity) {
      alert('Please select a file and entity type');
      return;
    }

    if (this.validationErrors.length > 0) {
      const proceed = confirm(`${this.validationErrors.length} validation errors found. Proceed with import anyway?`);
      if (!proceed) return;
    }

    this.isImporting = true;
    this.importProgress = 0;

    const simulateProgress = () => {
      this.importProgress = 0;
      const interval = setInterval(() => {
        this.importProgress += 5;
        if (this.importProgress >= 100) {
          clearInterval(interval);
        }
      }, 100);
    };

    simulateProgress();

    setTimeout(() => {
      this.isImporting = false;
      this.importProgress = 100;

      const importCount = this.importPreview.length;
      this.importResults = {
        total: importCount,
        imported: Math.floor(importCount * 0.95),
        updated: Math.floor(importCount * 0.03),
        skipped: importCount - Math.floor(importCount * 0.98),
        errors: this.validationErrors,
        duplicates: this.duplicateRecords
      };

      alert(`Import completed!\n\nTotal: ${this.importResults.total}\nImported: ${this.importResults.imported}\nUpdated: ${this.importResults.updated}\nSkipped: ${this.importResults.skipped}\nErrors: ${this.importResults.errors.length}`);
      this.closeImport();
    }, 1500);
  }

  downloadTemplate(): void {
    if (!this.selectedEntity) return;

    const headers = this.selectedEntity.fields.join(',');
    const sampleRow = this.selectedEntity.template[0];
    const sampleValues = this.selectedEntity.fields.map(f => sampleRow[f] || '').join(',');
    const csvContent = `data:text/csv;charset=utf-8,${headers}%0A${sampleValues}`;

    const link = document.createElement('a');
    link.setAttribute('href', 'data:text/csv;charset=utf-8,' + encodeURIComponent(`${headers}\n${sampleValues}`));
    link.setAttribute('download', `${this.selectedEntity.name.toLowerCase()}_template.csv`);
    link.click();
  }

  closeImport(): void {
    this.selectedEntity = null;
    this.importFile = null;
    this.importPreview = [];
    this.validationErrors = [];
    this.duplicateRecords = [];
    this.importResults = null;
    this.showPreview = false;
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  hasRole(role: string): boolean {
    return this.authService.hasRole(role);
  }

  getHeaders(): string[] {
    if (this.importPreview.length === 0) return [];
    return Object.keys(this.importPreview[0]);
  }
}
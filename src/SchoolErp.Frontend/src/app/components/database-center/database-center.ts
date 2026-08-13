import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { DatabaseCenterService, TableInfo, RecordData, TableStatistics, ImportStrategy, ImportResult } from '../../services/database-center.service';
import { Subscription } from 'rxjs';

interface Table {
  name: string;
  entityType: string;
  recordCount: number;
  sizeKb: number;
}

@Component({
  selector: 'app-database-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './database-center.html',
  styleUrls: ['./database-center.css']
})
export class DatabaseCenterComponent implements OnInit, OnDestroy {
  currentUser: any = null;
  tables: Table[] = [];
  selectedTable: string | null = null;
  records: RecordData[] = [];
  tableStats: TableStatistics | null = null;
  isLoading = false;
  isLoadingRecords = false;
  showImportModal = false;
  showExportModal = false;
  importFile: File | null = null;
  importPreview: any[] = [];
  importErrors: string[] = [];
  importDuplicates: any[] = [];
  importStrategy: ImportStrategy = 'insert';
  importResults: ImportResult | null = null;
  searchQuery = '';
  filterColumn = '';
  filterValue = '';
  sortBy = '';
  sortDirection: 'asc' | 'desc' = 'asc';
  currentPage = 1;
  pageSize = 20;
  totalRecords = 0;
  private subscriptions: Subscription = new Subscription();

  constructor(
    public authService: AuthService,
    private dbService: DatabaseCenterService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    if (!this.authService.hasAnyRole(['Admin', 'SuperAdmin'])) {
      alert('Access denied. Administrator privileges required.');
      this.router.navigate(['/dashboard']);
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

    this.loadTables();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadTables(): void {
    this.isLoading = true;
    this.dbService.getTables().subscribe({
      next: (data) => {
        this.tables = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load tables', err);
        this.isLoading = false;
      }
    });
  }

  selectTable(tableName: string): void {
    this.selectedTable = tableName;
    this.records = [];
    this.tableStats = null;
    this.loadRecords();
  }

  loadRecords(): void {
    if (!this.selectedTable) return;

    this.isLoadingRecords = true;
    const params: any = {
      page: this.currentPage,
      pageSize: this.pageSize
    };

    if (this.filterColumn && this.filterValue) {
      params.filterColumn = this.filterColumn;
      params.filterValue = this.filterValue;
    }

    if (this.sortBy) {
      params.sortBy = this.sortBy;
      params.sortDirection = this.sortDirection;
    }

    this.dbService.getRecords(this.selectedTable, params).subscribe({
      next: (data) => {
        this.records = data.records;
        this.totalRecords = data.totalCount;
        this.isLoadingRecords = false;
      },
      error: (err) => {
        console.error('Failed to load records', err);
        this.isLoadingRecords = false;
      }
    });
  }

  loadTableStatistics(): void {
    if (!this.selectedTable) return;

    this.dbService.getTableStatistics(this.selectedTable).subscribe({
      next: (data) => {
        this.tableStats = data;
      },
      error: (err) => {
        console.error('Failed to load statistics', err);
      }
    });
  }

  searchRecords(): void {
    this.currentPage = 1;
    this.loadRecords();
  }

  filterRecords(): void {
    this.currentPage = 1;
    this.loadRecords();
  }

  sortRecords(column: string): void {
    if (this.sortBy === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDirection = 'asc';
    }
    this.currentPage = 1;
    this.loadRecords();
  }

  openImportModal(): void {
    if (!this.selectedTable) return;
    this.importFile = null;
    this.importPreview = [];
    this.importErrors = [];
    this.importDuplicates = [];
    this.importStrategy = 'insert';
    this.showImportModal = true;
  }

  closeImportModal(): void {
    this.showImportModal = false;
  }

  openExportModal(): void {
    if (!this.selectedTable) return;
    this.showExportModal = true;
  }

  closeExportModal(): void {
    this.showExportModal = false;
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.importFile = file;
      this.previewImportFile(file);
    }
  }

  previewImportFile(file: File): void {
    this.importPreview = [];
    this.importErrors = [];
    this.importDuplicates = [];

    const reader = new FileReader();
    reader.onload = (e) => {
      const text = e.target?.result as string;
      const lines = text.split('\n').filter(l => l.trim());
      const headers = lines[0]?.split(',') || [];
      
      for (let i = 1; i < Math.min(lines.length, 10); i++) {
        const values = lines[i].split(',');
        const row: any = {};
        headers.forEach((header, idx) => {
          row[header.trim()] = values[idx]?.trim() || '';
        });
        this.importPreview.push(row);
      }
    };
    reader.readAsText(file);
  }

  executeImport(): void {
    if (!this.importFile || !this.selectedTable) return;

    this.dbService.importRecords(this.selectedTable, this.importFile, this.importStrategy).subscribe({
      next: (result) => {
        this.importResults = result;
        this.importErrors = result.errors || [];
        this.importDuplicates = result.duplicates || [];
        this.loadRecords();
      },
      error: (err) => {
        alert('Import failed: ' + err.message);
      }
    });
  }

  exportRecords(): void {
    if (!this.selectedTable) return;
    this.dbService.exportRecords(this.selectedTable).subscribe({
      next: () => {
        alert('Export completed');
      },
      error: (err) => {
        alert('Export failed: ' + err.message);
      }
    });
  }

  deleteRecord(id: string): void {
    if (!this.selectedTable || !confirm('Are you sure you want to delete this record?')) return;

    this.dbService.deleteRecord(this.selectedTable!, id).subscribe({
      next: () => {
        this.loadRecords();
      },
      error: (err) => {
        alert('Delete failed: ' + err.message);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  hasRole(roles: string | string[]): boolean {
    if (Array.isArray(roles)) {
      return this.authService.hasAnyRole(roles);
    }
    return this.authService.hasRole(roles);
  }

  getColumns(): string[] {
    if (!this.selectedTable || this.records.length === 0) return [];
    return Object.keys(this.records[0]);
  }

  formatValue(value: any): string {
    if (value === null || value === undefined) return '';
    if (typeof value === 'boolean') return value ? 'Yes' : 'No';
    if (typeof value === 'object') return JSON.stringify(value);
    return String(value);
  }

  getTotalPages(): number {
    return Math.ceil(this.totalRecords / this.pageSize);
  }

  getPageNumbers(): number[] {
    const totalPages = this.getTotalPages();
    const pages: number[] = [];
    for (let i = 1; i <= totalPages; i++) {
      pages.push(i);
    }
    return pages;
  }

  goToPage(page: number): void {
    this.currentPage = page;
  }

  getImportHeaders(): string[] {
    if (this.importPreview.length === 0) return [];
    return Object.keys(this.importPreview[0]);
  }
}
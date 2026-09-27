import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { AuditLogsService, AuditLogDto } from '../../services/audit-logs.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-logs.html',
  styleUrls: ['./audit-logs.css']
})
export class AuditLogsComponent implements OnInit {
  currentUser: any = null;
  auditLogs: AuditLogDto[] = [];
  filteredLogs: AuditLogDto[] = [];
  isLoading = true;
  searchQuery = '';
  filterAction = '';
  filterModule = '';
  filterUser = '';
  currentPage = 1;
  pageSize = 20;
  totalLogs = 0;
  showDetails = false;
  selectedLog: AuditLogDto | null = null;

  constructor(
    public authService: AuthService,
    private auditLogsService: AuditLogsService,
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

    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.isLoading = true;

    this.auditLogsService.getAuditLogs().subscribe({
      next: (result) => {
        this.auditLogs = result.items;
        this.filteredLogs = result.items;
        this.totalLogs = result.totalCount;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading audit logs:', err);
        this.isLoading = false;
      }
    });
  }

  filterLogs(): void {
    this.currentPage = 1;
    const query = this.searchQuery.toLowerCase().trim();
    const action = this.filterAction.toLowerCase().trim();
    const module = this.filterModule.toLowerCase().trim();
    const user = this.filterUser.toLowerCase().trim();

    this.filteredLogs = this.auditLogs.filter(log => {
      const matchesSearch =
        log.userName?.toLowerCase().includes(query) ||
        log.userEmail?.toLowerCase().includes(query) ||
        log.action?.toLowerCase().includes(query) ||
        log.description?.toLowerCase().includes(query);

      const matchesAction = action ? log.action?.toLowerCase() === action : true;
      const matchesModule = module ? log.module?.toLowerCase() === module : true;
      const matchesUser = user ? log.userName?.toLowerCase() === user : true;

      return matchesSearch && matchesAction && matchesModule && matchesUser;
    });

    this.totalLogs = this.filteredLogs.length;
  }

  viewLogDetails(log: AuditLogDto): void {
    this.selectedLog = log;
    this.showDetails = true;
  }

  closeDetails(): void {
    this.selectedLog = null;
    this.showDetails = false;
  }

  getActionBadge(action: string): string {
    const badges: { [key: string]: string } = {
      'LOGIN': 'success',
      'REGISTER': 'primary',
      'UPDATE': 'warning',
      'DELETE': 'danger',
      'BACKUP_COMPLETED': 'info',
      'ATTENDANCE_MARKED': 'secondary',
      'PAYMENT': 'success',
      'CHANGE_PASSWORD': 'warning',
      'UPDATE_PROFILE': 'info'
    };
    return badges[action] || 'secondary';
  }

  getActionIcon(action: string): string {
    const icons: { [key: string]: string } = {
      'LOGIN': '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">lock</span>',
      'REGISTER': '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">add</span>',
      'UPDATE': '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">edit</span>️',
      'DELETE': '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">delete</span>️',
      'BACKUP_COMPLETED': '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">save</span>',
      'ATTENDANCE_MARKED': '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">assignment</span>',
      'PAYMENT': '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">payments</span>',
      'CHANGE_PASSWORD': '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">key</span>',
      'UPDATE_PROFILE': '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">person</span>'
    };
    return icons[action] || '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">note</span>';
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  hasRole(roles: string | string[]): boolean {
    if (Array.isArray(roles)) {
      return this.authService.hasAnyRole(roles);
    }
    return this.authService.hasRole(roles);
  }

  get paginatedLogs(): AuditLogDto[] {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    return this.filteredLogs.slice(startIndex, startIndex + this.pageSize);
  }

  getTotalPages(): number {
    return Math.ceil(this.filteredLogs.length / this.pageSize);
  }

  min(a: number, b: number): number {
    return Math.min(a, b);
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

  exportToExcel(): void {
    alert('Export to Excel functionality would be implemented here.');
  }

  exportToPDF(): void {
    alert('Export to PDF functionality would be implemented here.');
  }
}
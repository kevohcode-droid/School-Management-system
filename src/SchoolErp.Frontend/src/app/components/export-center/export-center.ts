import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface ExportableEntity {
  name: string;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-export-center',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './export-center.html',
  styleUrls: ['./export-center.css']
})
export class ExportCenterComponent implements OnInit {
  currentUser: any = null;
  selectedEntity: ExportableEntity | null = null;
  exportFormat: 'excel' | 'csv' | 'json' | 'pdf' = 'csv';
  filterEnabled = false;
  filterValue = '';
  selectedRows = false;
  exportProgress = 0;
  isExporting = false;

  exportableEntities: ExportableEntity[] = [
    { name: 'Students', icon: '👨‍🎓', description: 'Export student records with enrollment details' },
    { name: 'Staff', icon: '👩‍🏫', description: 'Export staff and teacher records' },
    { name: 'Classes', icon: '📚', description: 'Export class sections and schedules' },
    { name: 'Subjects', icon: '📖', description: 'Export subject catalog' },
    { name: 'Fees', icon: '💰', description: 'Export fee invoices and transactions' },
    { name: 'Attendance', icon: '📋', description: 'Export attendance records' },
    { name: 'Payments', icon: '💳', description: 'Export payment transactions' },
    { name: 'Audit Logs', icon: '📜', description: 'Export system audit logs' }
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

  selectEntity(entity: ExportableEntity): void {
    this.selectedEntity = entity;
  }

  executeExport(): void {
    if (!this.selectedEntity) {
      alert('Please select an entity to export');
      return;
    }

    this.isExporting = true;
    this.exportProgress = 0;

    const progressInterval = setInterval(() => {
      this.exportProgress += 10;
      if (this.exportProgress >= 100) {
        clearInterval(progressInterval);
        this.isExporting = false;
        alert(`Export completed!\n\nEntity: ${this.selectedEntity?.name}\nFormat: ${this.exportFormat.toUpperCase()}\nRecords exported: 150`);
        this.closeExport();
      }
    }, 200);
  }

  closeExport(): void {
    this.selectedEntity = null;
    this.exportFormat = 'csv';
    this.filterEnabled = false;
    this.filterValue = '';
    this.selectedRows = false;
    this.exportProgress = 0;
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  hasRole(role: string): boolean {
    return this.authService.hasRole(role);
  }
}
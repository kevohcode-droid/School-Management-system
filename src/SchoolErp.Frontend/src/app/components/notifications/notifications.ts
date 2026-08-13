import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface Notification {
  id: string;
  type: 'info' | 'success' | 'warning' | 'error';
  title: string;
  message: string;
  date: string;
  isRead: boolean;
  link?: string;
}

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notifications.html',
  styleUrls: ['./notifications.css']
})
export class NotificationsComponent implements OnInit {
  currentUser: any = null;
  notifications: Notification[] = [];
  filteredNotifications: Notification[] = [];
  isLoading = true;
  filterType = '';
  showUnreadOnly = false;
  currentPage = 1;
  pageSize = 20;

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

    this.loadNotifications();
  }

  loadNotifications(): void {
    this.isLoading = true;

    const mockNotifications: Notification[] = [
      {
        id: '1',
        type: 'success',
        title: 'New Student Registered',
        message: 'John Doe has been successfully registered as a new student.',
        date: '2026-07-06T10:30:00Z',
        isRead: false
      },
      {
        id: '2',
        type: 'info',
        title: 'Fee Payment Received',
        message: 'KSh 50,000 received from Jane Smith for Term Fees.',
        date: '2026-07-06T09:15:00Z',
        isRead: false
      },
      {
        id: '3',
        type: 'warning',
        title: 'Attendance Not Submitted',
        message: 'Class 1B attendance has not been submitted yet.',
        date: '2026-07-06T08:00:00Z',
        isRead: true
      },
      {
        id: '4',
        type: 'success',
        title: 'Backup Completed',
        message: 'Daily database backup completed successfully.',
        date: '2026-07-06T02:00:00Z',
        isRead: true
      },
      {
        id: '5',
        type: 'error',
        title: 'Failed Login Attempt',
        message: 'Failed login attempt from IP 192.168.1.200',
        date: '2026-07-05T23:45:00Z',
        isRead: true
      },
      {
        id: '6',
        type: 'info',
        title: 'Database Warning',
        message: 'Database storage is at 85% capacity.',
        date: '2026-07-05T20:00:00Z',
        isRead: true
      }
    ];

    setTimeout(() => {
      this.notifications = mockNotifications;
      this.filteredNotifications = mockNotifications;
      this.isLoading = false;
    }, 500);
  }

  filterNotifications(): void {
    this.currentPage = 1;
    const type = this.filterType.toLowerCase().trim();
    const unreadOnly = this.showUnreadOnly;

    this.filteredNotifications = this.notifications.filter(n => {
      const matchesType = type ? n.type === type : true;
      const matchesUnread = unreadOnly ? !n.isRead : true;
      return matchesType && matchesUnread;
    });
  }

  onFilterTypeChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.filterType = target.value;
    this.filterNotifications();
  }

  markAsRead(notification: Notification): void {
    notification.isRead = true;
  }

  markAllAsRead(): void {
    this.filteredNotifications.forEach(n => n.isRead = true);
    this.notifications.forEach(n => n.isRead = true);
  }

  deleteNotification(id: string): void {
    if (confirm('Are you sure you want to delete this notification?')) {
      this.notifications = this.notifications.filter(n => n.id !== id);
      this.filterNotifications();
    }
  }

  navigateTo(notification: Notification): void {
    notification.isRead = true;
    if (notification.link) {
      this.router.navigate([notification.link]);
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }

  getNotificationIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'success': '✅',
      'info': 'ℹ️',
      'warning': '⚠️',
      'error': '❌'
    };
    return icons[type] || '📢';
  }

  getTypeBadgeClass(type: string): string {
    return type;
  }

  hasRole(role: string): boolean {
    return this.authService.hasRole(role);
  }
}
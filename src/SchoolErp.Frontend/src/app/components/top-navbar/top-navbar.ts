import {
  Component, OnInit, OnDestroy, Input, Output, EventEmitter,
  HostListener, signal, ElementRef, ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';

export interface BreadcrumbItem {
  label: string;
  route?: string;
}

// Map of routes <span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">arrow_forward</span> breadcrumb labels
const ROUTE_LABELS: Record<string, string> = {
  '':                   'Home',
  'dashboard':          'Dashboard',
  'students':           'Students',
  'staff':              'Staff',
  'classes':            'Classes',
  'attendance':         'Attendance',
  'fees':               'Fee Management',
  'payments':           'Transactions',
  'fee-structures':     'Fee Structures',
  'reports':            'Reports',
  'analytics':          'Analytics',
  'academics':          'Academics',
  'performance':        'Performance',
  'settings':           'Settings',
  'profile':            'Profile',
  'change-password':    'Change Password',
  'preferences':        'Preferences',
  'notifications':      'Notifications',
  'audit-logs':         'Audit Logs',
  'database-center':    'Database Center',
  'import-center':      'Import Center',
  'export-center':      'Export Center',
  'tenants':            'Tenants',
};

@Component({
  selector: 'app-top-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './top-navbar.html',
  styleUrls: ['./top-navbar.css']
})
export class TopNavbarComponent implements OnInit, OnDestroy {
  @Input() sidebarCollapsed = false;
  @Output() menuToggle = new EventEmitter<void>();

  @ViewChild('searchInput') searchInput?: ElementRef<HTMLInputElement>;

  currentUser: any = null;
  breadcrumbs: BreadcrumbItem[] = [];
  profileOpen = signal(false);
  searchQuery = '';
  private routeSub: Subscription = new Subscription();

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const cached = this.authService.getCurrentUserFromStorage();
    if (cached) this.currentUser = cached;
    this.authService.getCurrentUser().subscribe({
      next: u => { this.currentUser = u; },
      error: () => {}
    });

    this.routeSub = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe(() => this.buildBreadcrumbs());
    this.buildBreadcrumbs();
  }

  ngOnDestroy(): void { this.routeSub.unsubscribe(); }

  private buildBreadcrumbs(): void {
    const url = this.router.url.split('?')[0];
    const segments = url.split('/').filter(s => s.length > 0);
    const crumbs: BreadcrumbItem[] = [{ label: 'Home', route: '/dashboard' }];
    let accumulated = '';
    for (const seg of segments) {
      accumulated += '/' + seg;
      const label = ROUTE_LABELS[seg] ?? this.capitalise(seg);
      crumbs.push({ label, route: accumulated });
    }
    // Don't show "Home" alone when we're already on dashboard
    this.breadcrumbs = crumbs.length > 1 ? crumbs : crumbs;
  }

  private capitalise(s: string): string {
    return s.replace(/-/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
  }

  get userInitials(): string {
    if (!this.currentUser) return '?';
    const fn = this.currentUser.fullName || this.currentUser.email || '';
    return fn.split(' ').map((w: string) => w[0]).join('').slice(0, 2).toUpperCase();
  }

  get userRole(): string {
    const roles: string[] = this.currentUser?.roles ?? [];
    if (roles.includes('SuperAdmin')) return 'Super Admin';
    if (roles.includes('Admin')) return 'Admin';
    if (roles.includes('Teacher')) return 'Teacher';
    if (roles.includes('Student')) return 'Student';
    if (roles.includes('Parent')) return 'Parent';
    return roles[0] ?? 'User';
  }

  toggleProfile(): void { this.profileOpen.update(v => !v); }
  closeProfile(): void  { this.profileOpen.set(false); }

  navigateTo(route: string): void {
    this.router.navigate([route]);
    this.closeProfile();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  onSearch(event: Event): void {
    const q = (event.target as HTMLInputElement).value.trim();
    if (q.length > 1) {
      // Emit search globally or navigate to a search route
      // Extendable: emit an output or call a search service
    }
  }

  // Global keyboard shortcut: / focuses search
  @HostListener('document:keydown', ['$event'])
  onGlobalKey(e: KeyboardEvent): void {
    if (e.key === '/' && document.activeElement?.tagName !== 'INPUT' && document.activeElement?.tagName !== 'TEXTAREA') {
      e.preventDefault();
      this.searchInput?.nativeElement.focus();
    }
    if (e.key === 'Escape') { this.closeProfile(); }
  }

  // Click outside closes profile dropdown
  @HostListener('document:click', ['$event'])
  onDocClick(e: MouseEvent): void {
    const target = e.target as HTMLElement;
    if (!target.closest('.profile-menu-wrap')) this.closeProfile();
  }
}

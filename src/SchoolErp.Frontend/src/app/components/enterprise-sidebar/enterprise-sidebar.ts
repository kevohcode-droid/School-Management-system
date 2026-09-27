import {
  Component, OnInit, OnDestroy, HostListener, Output, EventEmitter, Input, signal, computed
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';

export interface SidebarItem {
  label: string;
  icon: string;
  route?: string;
  children?: SidebarItem[];
  roles?: string[];
  badge?: number;
  id: string;
}

@Component({
  selector: 'app-enterprise-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './enterprise-sidebar.html',
  styleUrls: ['./enterprise-sidebar.css']
})
export class EnterpriseSidebarComponent implements OnInit, OnDestroy {
  @Output() collapsedChange = new EventEmitter<boolean>();
  @Output() requestCloseMobile = new EventEmitter<void>();

  @Input() set mobileOpenInput(value: boolean) { this._mobileOpen.set(value); }
  @Input() set collapsedInput(value: boolean) { this._collapsed.set(value); }

  currentUser: any = null;
  expandedGroups = new Set<string>();
  private routeSub: Subscription = new Subscription();

  private _collapsed = signal(false);
  private _mobileOpen = signal(false);

  collapsed = computed(() => this._collapsed());
  mobileOpen = computed(() => this._mobileOpen());

  allItems: SidebarItem[] = [
    {
      id: 'dashboard', label: 'Dashboard', route: '/dashboard',
      icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6'
    },
    {
      id: 'academics', label: 'Academics', icon: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253',
      children: [
        { id: 'students', label: 'Students', route: '/students', roles: ['Admin','SuperAdmin','Teacher'], icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' },
        { id: 'performance', label: 'Performance', route: '/academics/performance', roles: ['Admin', 'SuperAdmin', 'Teacher', 'Student'], icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z' },
        { id: 'student-attendance', label: 'My Attendance', route: '/student/attendance', roles: ['Student'], icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4' },
        { id: 'student-fees', label: 'My Fees', route: '/student/fees', roles: ['Student'], icon: 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z' },
        { id: 'student-profile', label: 'My Profile', route: '/student/profile', roles: ['Student'], icon: 'M5.121 17.804A13.937 13.937 0 0112 16c2.5 0 4.847.655 6.879 1.804M15 10a3 3 0 11-6 0 3 3 0 016 0zm6 2a9 9 0 11-18 0 9 9 0 0118 0z' },
        { id: 'staff', label: 'Staff', route: '/staff', roles: ['Admin','SuperAdmin'], icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z' },
        { id: 'classes', label: 'Classes', route: '/classes', roles: ['Admin', 'SuperAdmin', 'Teacher'], icon: 'M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10' },
      ]
    },
    {
      id: 'attendance', label: 'Attendance', route: '/attendance', roles: ['Admin', 'SuperAdmin', 'Teacher'],
      icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4'
    },
    {
      id: 'finance', label: 'Finance', roles: ['Admin', 'SuperAdmin', 'Accountant'], icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
      children: [
        { id: 'fees', label: 'Fee Management', route: '/fees', roles: ['Admin', 'SuperAdmin', 'Accountant'], icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' },
        { id: 'payments', label: 'Transactions', route: '/payments', roles: ['Admin', 'SuperAdmin', 'Accountant'], icon: 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z' },
      ]
    },
    {
      id: 'reports', label: 'Reports', route: '/reports', roles: ['Admin', 'SuperAdmin', 'Teacher', 'Accountant'],
      icon: 'M9 17v-2m3 2v-4m3 4v-6m2 10H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z'
    },
    {
      id: 'notifications', label: 'Notifications', route: '/notifications',
      icon: 'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9'
    },
    {
      id: 'admin-group', label: 'Administration', icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z',
      roles: ['Admin','SuperAdmin'],
      children: [
        { id: 'settings', label: 'Settings', route: '/settings', roles: ['Admin','SuperAdmin'], icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z' },
        { id: 'audit-logs', label: 'Audit Logs', route: '/audit-logs', roles: ['Admin','SuperAdmin'], icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2' },
        { id: 'database-center', label: 'Database Center', route: '/database-center', roles: ['SuperAdmin'], icon: 'M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4m0 5c0 2.21-3.582 4-8 4s-8-1.79-8-4' },
        { id: 'import-center', label: 'Import', route: '/import-center', roles: ['Admin','SuperAdmin'], icon: 'M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12' },
        { id: 'export-center', label: 'Export', route: '/export-center', roles: ['Admin','SuperAdmin'], icon: 'M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4' },
        { id: 'tenants', label: 'Tenants', route: '/tenants', roles: ['SuperAdmin'], icon: 'M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-2 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4' },
      ]
    },
  ];

  visibleItems: SidebarItem[] = [];

  constructor(public authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.loadUser();
    this.routeSub = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe(() => this.autoExpandActive());
    this.autoExpandActive();
  }

  ngOnDestroy(): void { this.routeSub.unsubscribe(); }

  private loadUser(): void {
    const cached = this.authService.getCurrentUserFromStorage();
    if (cached) { this.currentUser = cached; this.buildVisibleItems(); }
    this.authService.getCurrentUser().subscribe({
      next: u => { this.currentUser = u; this.authService.saveCurrentUser(u); this.buildVisibleItems(); },
      error: () => this.buildVisibleItems()
    });
  }

  private buildVisibleItems(): void {
    const userRoles: string[] = this.currentUser?.roles ?? [];
    const isStudent = userRoles.includes('Student');
    const canSee = (item: SidebarItem) =>
      !item.roles || item.roles.some(r => userRoles.includes(r));

    this.visibleItems = this.allItems
      .filter(canSee)
      .map(item => ({
        ...item,
        route: isStudent && item.id === 'dashboard' ? '/student' : item.route,
        children: item.children?.filter(canSee).map(child =>
          isStudent && child.id === 'performance'
            ? { ...child, route: '/student/performance' }
            : child
        )
      }))
      .filter(item => !item.children || item.children.length > 0);
  }

  private autoExpandActive(): void {
    const url = this.router.url;
    this.visibleItems.forEach(item => {
      if (item.children?.some(c => c.route && url.startsWith(c.route))) {
        this.expandedGroups.add(item.id);
      }
    });
  }

  toggleCollapse(): void {
    this._collapsed.update(v => !v);
    this.collapsedChange.emit(this._collapsed());
    if (this._collapsed()) this.expandedGroups.clear();
  }

  toggleGroup(item: SidebarItem): void {
    if (this.expandedGroups.has(item.id)) {
      this.expandedGroups.delete(item.id);
    } else {
      this.expandedGroups.add(item.id);
    }
  }

  isGroupExpanded(item: SidebarItem): boolean {
    return this.expandedGroups.has(item.id);
  }

  isActive(route: string): boolean {
    return this.router.url === route || this.router.url.startsWith(route + '/');
  }

  isGroupActive(item: SidebarItem): boolean {
    return !!item.children?.some(c => c.route && this.isActive(c.route));
  }

  navigate(route: string): void { this.router.navigate([route]); }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private setMobileOpen(value: boolean): void {
    this._mobileOpen.set(value);
  }

  onKeydown(event: KeyboardEvent, item: SidebarItem): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      if (item.children?.length) { this.toggleGroup(item); }
      else if (item.route) { this.navigate(item.route); }
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void { this.setMobileOpen(false); }
}
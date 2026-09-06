import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';

interface SidebarItem {
  label: string;
  icon: string;
  route?: string;
  children?: SidebarItem[];
  hasChildren?: boolean;
  adminOnly?: boolean;
}

@Component({
  selector: 'app-enterprise-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './enterprise-sidebar.html',
  styleUrls: ['./enterprise-sidebar.css']
})
export class EnterpriseSidebarComponent implements OnInit {
  currentUser: any = null;
  activeItem: string | null = null;
  expandedItems: Set<string> = new Set();
  private routeSubscription: Subscription = new Subscription();
  summary: any = null;

  sidebarItems: SidebarItem[] = [];

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const cachedUser = this.authService.getCurrentUserFromStorage();
    if (cachedUser) {
      this.currentUser = cachedUser;
    }

    this.authService.getCurrentUser().subscribe({
      next: (user) => {
        this.currentUser = user;
        this.authService.saveCurrentUser(user);
        this.buildSidebarItems(user);
      },
      error: () => {
        this.buildSidebarItems(this.currentUser);
      }
    });

    this.routeSubscription = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.setActiveItem(event.url);
    });

    this.setActiveItem(this.router.url);
  }

  ngOnDestroy(): void {
    this.routeSubscription.unsubscribe();
  }

  buildSidebarItems(user: any): void {
    const roles = user.roles || [];
    const isAdmin = roles.includes('Admin') || roles.includes('SuperAdmin');
    const isSuperAdmin = roles.includes('SuperAdmin');

    this.sidebarItems = [
      { label: 'Dashboard', icon: '📊', route: '/dashboard' },
      {
        label: 'Academics',
        icon: '🎓',
        hasChildren: true,
        children: [
          { label: 'Students', icon: '👨‍🎓', route: '/students' },
          { label: 'Staff', icon: '👩‍🏫', route: '/staff' },
          { label: 'Teachers', icon: '👩‍🏫', route: '/staff' },
          { label: 'Classes', icon: '📚', route: '/classes' },
          { label: 'Subjects', icon: '📖', route: '/subjects' },
          { label: 'Departments', icon: '🏢', route: '/departments' },
          { label: 'Academic Years', icon: '📅', route: '/academic-years' },
          { label: 'Terms', icon: '🗓️', route: '/terms' },
          { label: 'Streams', icon: '🔄', route: '/streams' }
        ]
      },
      { label: 'Admissions', icon: '🎯', route: '/admissions' },
      { label: 'Attendance', icon: '📋', route: '/attendance' },
      { label: 'Examinations', icon: '📝', route: '/examinations' },
      {
        label: 'Finance',
        icon: '💳',
        hasChildren: true,
        children: [
          { label: 'Transactions & Balances', icon: '🧾', route: '/payments' },
          { label: 'Fee Management', icon: '📋', route: '/fees' },
          { label: 'Payment Methods', icon: '⚙️', route: '/payment-methods' }
        ]
      },
      { label: 'Library', icon: '📚', route: '/library' },
      { label: 'Inventory', icon: '📦', route: '/inventory' },
      { label: 'Transport', icon: '🚌', route: '/transport' },
      { label: 'Hostel', icon: '🏨', route: '/hostel' },
      { label: 'Parents', icon: '👨‍👩‍👧', route: '/parents' },
      { label: 'Reports', icon: '📊', route: '/reports' },
      {
        label: 'Database Center',
        icon: '🗄️',
        route: '/database-center',
        adminOnly: true
      },
      { label: 'Audit Logs', icon: '📜', route: '/audit-logs', adminOnly: true },
      { label: 'Notifications', icon: '🔔', route: '/notifications' },
      { label: 'Import Center', icon: '📥', route: '/import-center' },
      { label: 'Export Center', icon: '📤', route: '/export-center' },
      {
        label: 'User Management',
        icon: '🛡️',
        hasChildren: true,
        children: [
          { label: 'Users', icon: '👤', route: '/users' },
          { label: 'Roles & Permissions', icon: '🔐', route: '/roles-permissions' }
        ],
        adminOnly: true
      },
      { label: 'Settings', icon: '⚙️', route: '/settings' }
    ];

    if (!isSuperAdmin) {
      this.sidebarItems = this.sidebarItems.filter(item => {
        if (item.adminOnly && !isAdmin) return false;
        if (item.children) {
          item.children = item.children.filter(child => {
            if (child.adminOnly && !isAdmin) return false;
            return true;
          });
          return item.children.length > 0 || !item.adminOnly;
        }
        return true;
      });
    }
  }

  toggleExpand(item: SidebarItem): void {
    const key = item.label;
    if (this.expandedItems.has(key)) {
      this.expandedItems.delete(key);
    } else {
      this.expandedItems.add(key);
    }
  }

  hasChildrenExpanded(item: SidebarItem): boolean {
    return item.children ? this.expandedItems.has(item.label) : false;
  }

  setActiveItem(url: string): void {
    const parts = url.split('/').filter(p => p);
    if (parts.length > 0) {
      this.activeItem = '/' + parts[1];
    }
  }

  isActive(item: SidebarItem): boolean {
    if (!item.route) return false;
    return this.router.url === item.route || this.router.url.startsWith(item.route + '/');
  }

  navigateTo(route: string): void {
    this.router.navigate([route]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  hasRole(role: string): boolean {
    return this.authService.hasRole(role);
  }

  hasAnyRole(roles: string[]): boolean {
    return this.authService.hasAnyRole(roles);
  }
}
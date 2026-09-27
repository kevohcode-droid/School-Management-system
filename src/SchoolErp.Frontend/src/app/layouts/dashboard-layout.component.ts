import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { EnterpriseSidebarComponent } from '../components/enterprise-sidebar/enterprise-sidebar';
import { TopNavbarComponent } from '../components/top-navbar/top-navbar';
import { AuthService } from '../services/auth.service';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, EnterpriseSidebarComponent, TopNavbarComponent],
  template: `
    <a href="#main-content" class="skip-link">Skip to main content</a>

    <div class="app-shell">
      <app-enterprise-sidebar
        [mobileOpenInput]="sidebarMobileOpen()"
        [collapsedInput]="sidebarCollapsed()"
        (collapsedChange)="onSidebarCollapsed($event)"
        (requestCloseMobile)="closeMobileSidebar()"
      ></app-enterprise-sidebar>

      <div class="app-main" [class.sidebar-collapsed]="sidebarCollapsed()">
        <app-top-navbar
          [sidebarCollapsed]="sidebarCollapsed()"
          (menuToggle)="toggleMobileSidebar()"
        ></app-top-navbar>

        <div class="app-content" id="main-content" tabindex="-1">
          <router-outlet></router-outlet>
        </div>
      </div>
    </div>
  `
})
export class DashboardLayoutComponent implements OnInit, OnDestroy {
  @Input() title = 'School ERP';
  @Output() logout = new EventEmitter<void>();

  sidebarCollapsed = signal(false);
  sidebarMobileOpen = signal(false);
  private routeSub: Subscription = new Subscription();

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    this.routeSub = this.router.events.pipe(
      filter(e => e instanceof NavigationEnd)
    ).subscribe(() => {
      this.sidebarMobileOpen.set(false);
    });
  }

  ngOnDestroy(): void {
    this.routeSub.unsubscribe();
  }

  onSidebarCollapsed(collapsed: boolean): void {
    this.sidebarCollapsed.set(collapsed);
  }

  toggleMobileSidebar(): void {
    this.sidebarMobileOpen.update(v => !v);
  }

  closeMobileSidebar(): void {
    this.sidebarMobileOpen.set(false);
  }
}
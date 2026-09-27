import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login';
import { DashboardComponent } from './components/dashboard/dashboard';
import { AnalyticsComponent } from './components/analytics/analytics';
import { StudentsComponent } from './components/students/students';
import { TenantsComponent } from './components/tenants/tenants';
import { StaffComponent } from './components/staff/staff';
import { ClassesComponent } from './components/classes/classes';
import { DailyAttendanceComponent } from './components/attendance/daily-attendance';
import { SystemSettingsComponent } from './components/system-settings/system-settings.component';
import { ProfileComponent } from './components/profile/profile';
import { DatabaseCenterComponent } from './components/database-center/database-center';
import { ImportCenterComponent } from './components/import-center/import-center';
import { ExportCenterComponent } from './components/export-center/export-center';
import { AuditLogsComponent } from './components/audit-logs/audit-logs';
import { NotificationsComponent } from './components/notifications/notifications';
import { ChangePasswordComponent } from './components/change-password/change-password';
import { PreferencesComponent } from './components/preferences/preferences';
import { ResetPasswordComponent } from './components/reset-password/reset-password';
import { ReportsComponent } from './components/reports/reports';
import { FeesComponent } from './components/fees/fees';
import { PerformanceComponent } from './components/performance/performance';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';
import { AuthLayoutComponent } from './layouts/auth-layout.component';
import { DashboardLayoutComponent } from './layouts/dashboard-layout.component';
import { StudentDashboardComponent } from './components/student-portal/student-dashboard/student-dashboard';
import { StudentAttendanceComponent } from './components/student-portal/student-attendance/student-attendance';
import { StudentFeesComponent } from './components/student-portal/student-fees/student-fees';
import { StudentPerformanceComponent } from './components/student-portal/student-performance/student-performance';
import { StudentProfileComponent } from './components/student-portal/student-profile/student-profile';
import { studentGuard } from './guards/student.guard';

export const routes: Routes = [
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      { path: 'login', component: LoginComponent },
      { path: 'reset-password', component: ResetPasswordComponent },
      { path: '', redirectTo: '/login', pathMatch: 'full' }
    ]
  },
  {
    path: '',
    component: DashboardLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Teacher', 'Parent', 'Accountant', 'Staff'] } },
      { path: 'student', component: StudentDashboardComponent, canActivate: [studentGuard] },
      { path: 'student/attendance', component: StudentAttendanceComponent, canActivate: [studentGuard] },
      { path: 'student/fees', component: StudentFeesComponent, canActivate: [studentGuard] },
      { path: 'student/performance', component: StudentPerformanceComponent, canActivate: [studentGuard] },
      { path: 'student/profile', component: StudentProfileComponent, canActivate: [studentGuard] },
      { path: 'analytics', component: AnalyticsComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },
      { path: 'profile', component: ProfileComponent },
      { path: 'change-password', component: ChangePasswordComponent },
      { path: 'preferences', component: PreferencesComponent },
      { path: 'students', component: StudentsComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Teacher'] } },
      { path: 'tenants', component: TenantsComponent, canActivate: [roleGuard], data: { roles: ['SuperAdmin'] } },
      { path: 'staff', component: StaffComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },
      { path: 'classes', component: ClassesComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Teacher'] } },
      { path: 'attendance', component: DailyAttendanceComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Teacher'] } },
      {
        path: 'academics/performance',
        component: PerformanceComponent,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'SuperAdmin', 'Teacher', 'Student'] }
      },
      { path: 'reports', component: ReportsComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Teacher', 'Accountant'] } },
      { path: 'fees', component: FeesComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Accountant'] } },
      { path: 'payments', component: FeesComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Accountant'] } },
      { path: 'receipts', component: FeesComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Accountant'] } },
      { path: 'fee-structures', component: FeesComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Accountant'] } },
      { path: 'payment-methods', component: FeesComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin', 'Accountant'] } },
      { path: 'settings', component: SystemSettingsComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },
      { path: 'database-center', component: DatabaseCenterComponent, canActivate: [roleGuard], data: { roles: ['SuperAdmin'] } },
      { path: 'import-center', component: ImportCenterComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },
      { path: 'export-center', component: ExportCenterComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },
      { path: 'audit-logs', component: AuditLogsComponent, canActivate: [roleGuard], data: { roles: ['Admin', 'SuperAdmin'] } },
      { path: 'notifications', component: NotificationsComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: 'admin', redirectTo: '/settings', pathMatch: 'full' },
  { path: 'teacher', redirectTo: '/academics/performance', pathMatch: 'full' },
  { path: 'parent', redirectTo: '/academics/performance', pathMatch: 'full' },
  { path: 'performance', redirectTo: '/academics/performance', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];
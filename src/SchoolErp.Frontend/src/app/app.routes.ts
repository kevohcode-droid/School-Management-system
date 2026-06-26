import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login';
import { DashboardComponent } from './components/dashboard/dashboard';
import { StudentsComponent } from './components/students/students';
import { TenantsComponent } from './components/tenants/tenants';
import { StaffComponent } from './components/staff/staff';
import { ClassesComponent } from './components/classes/classes';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'students', component: StudentsComponent },
  { path: 'tenants', component: TenantsComponent },
  { path: 'staff', component: StaffComponent },
  { path: 'classes', component: ClassesComponent },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];

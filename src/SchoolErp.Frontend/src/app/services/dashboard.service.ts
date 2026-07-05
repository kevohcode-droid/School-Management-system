import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { DashboardSummary, DashboardPublic } from '../models/dashboard';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(private apiService: ApiService) {}

  getSummary(): Observable<DashboardSummary> {
    return this.apiService.get<DashboardSummary>('/dashboard/summary');
  }

  getPublicSummary(): Observable<DashboardPublic> {
    return this.apiService.get<DashboardPublic>('/dashboard/public-summary');
  }
}
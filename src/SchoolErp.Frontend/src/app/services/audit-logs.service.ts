import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface AuditLogDto {
  id: string;
  userId: string;
  userEmail: string;
  userName: string;
  userRole: string;
  action: string;
  module: string;
  entity: string;
  oldValue?: string;
  newValue?: string;
  description: string;
  ipAddress: string;
  browser: string;
  status: string;
  createdAtUtc: string;

  date: string;
  time: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface AuditLogFilter {
  searchQuery?: string;
  action?: string;
  module?: string;
  userId?: string;
  startDate?: string;
  endDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuditLogsService {
  constructor(private apiService: ApiService) {}

  getAuditLogs(filter: AuditLogFilter = {}): Observable<PaginatedResult<AuditLogDto>> {
    const params: any = {};
    if (filter.searchQuery) params.searchQuery = filter.searchQuery;
    if (filter.action) params.action = filter.action;
    if (filter.module) params.module = filter.module;
    if (filter.userId) params.userId = filter.userId;
    if (filter.startDate) params.startDate = filter.startDate;
    if (filter.endDate) params.endDate = filter.endDate;
    if (filter.pageNumber) params.pageNumber = filter.pageNumber;
    if (filter.pageSize) params.pageSize = filter.pageSize;

    return this.apiService.get<PaginatedResult<AuditLogDto>>('/audit-logs', params);
  }
}
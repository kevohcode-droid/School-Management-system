import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Tenant, CreateTenantRequest } from '../models/tenant';

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  constructor(private apiService: ApiService) {}

  getTenants(): Observable<Tenant[]> {
    return this.apiService.get<Tenant[]>('/tenants');
  }

  getTenant(id: string): Observable<Tenant> {
    return this.apiService.get<Tenant>(`/tenants/${id}`);
  }

  createTenant(request: CreateTenantRequest): Observable<Tenant> {
    return this.apiService.post<Tenant>('/tenants', request);
  }
}

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { UpdateSettingsRequest } from '../models/settings';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  constructor(private apiService: ApiService) {}

  getCategory(category: string): Observable<{ [key: string]: string }> {
    return this.apiService.get<{ [key: string]: string }>(`/settings/${category}`);
  }

  getSetting(category: string, key: string): Observable<{ key: string; value: string }> {
    return this.apiService.get<{ key: string; value: string }>(`/settings/key/${category}/${key}`);
  }

  updateSettings(request: UpdateSettingsRequest): Observable<{ message: string }> {
    return this.apiService.post<{ message: string }>('/settings', request);
  }
}

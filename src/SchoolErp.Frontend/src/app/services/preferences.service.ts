import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface PreferenceDto {
  appearance: string;
  language: string;
  timezone: string;
  emailNotifications: boolean;
  smsNotifications: boolean;
  pushNotifications: boolean;
  compactMode: boolean;
  showWelcomeBanner: boolean;
  defaultLandingPage: string;
  autoLogoutMinutes: number;
}

@Injectable({
  providedIn: 'root'
})
export class PreferencesService {
  constructor(private apiService: ApiService) {}

  getPreferences(): Observable<PreferenceDto> {
    return this.apiService.get<PreferenceDto>('/preferences');
  }

  updatePreferences(preferences: Partial<PreferenceDto>): Observable<any> {
    return this.apiService.put('/preferences', preferences);
  }
}
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../../services/settings.service';

@Component({
  selector: 'app-security-settings-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './security-settings.tab.html',
  styleUrls: ['./security-settings.tab.css']
})
export class SecuritySettingsTab implements OnInit {
  requireTwoFactor = false;
  sessionTimeoutMinutes = '60';
  isSaving = false;

  constructor(private settingsService: SettingsService) {}

  ngOnInit(): void {
    this.settingsService.getCategory('Security').subscribe({
      next: (data: { [key: string]: string }) => {
        this.requireTwoFactor = data['RequireTwoFactor'] === 'true';
        this.sessionTimeoutMinutes = data['SessionTimeoutMinutes'] ?? '60';
      },
      error: (err: any) => console.error('Failed to load security settings', err)
    });
  }

  save(): void {
    this.isSaving = true;
    const settings: { [key: string]: string } = {
      RequireTwoFactor: this.requireTwoFactor ? 'true' : 'false',
      SessionTimeoutMinutes: this.sessionTimeoutMinutes
    };

    this.settingsService.updateSettings({ category: 'Security', settings }).subscribe({
      next: () => {
        alert('Security settings saved successfully!');
        this.isSaving = false;
      },
      error: () => {
        alert('Failed to save settings.');
        this.isSaving = false;
      }
    });
  }
}

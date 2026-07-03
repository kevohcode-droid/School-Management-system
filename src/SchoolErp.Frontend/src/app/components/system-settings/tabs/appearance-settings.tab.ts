import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../../services/settings.service';

@Component({
  selector: 'app-appearance-settings-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './appearance-settings.tab.html',
  styleUrls: ['./appearance-settings.tab.css']
})
export class AppearanceSettingsTab implements OnInit {
  primaryColor = '#2563eb';
  isDarkModeEnabled = false;
  isSaving = false;

  readonly colorOptions = [
    { value: '#2563eb', label: 'Blue' },
    { value: '#059669', label: 'Green' },
    { value: '#d97706', label: 'Amber' },
    { value: '#dc2626', label: 'Red' },
    { value: '#7c3aed', label: 'Purple' },
    { value: '#db2777', label: 'Pink' }
  ];

  constructor(private settingsService: SettingsService) {}

  ngOnInit(): void {
    this.settingsService.getCategory('Appearance').subscribe({
      next: (data: { [key: string]: string }) => {
        this.primaryColor = data['PrimaryColor'] ?? '#2563eb';
        this.isDarkModeEnabled = data['IsDarkModeEnabled'] === 'true';
      },
      error: (err: any) => console.error('Failed to load appearance settings', err)
    });
  }

  save(): void {
    this.isSaving = true;
    const settings: { [key: string]: string } = {
      PrimaryColor: this.primaryColor,
      IsDarkModeEnabled: this.isDarkModeEnabled ? 'true' : 'false'
    };

    this.settingsService.updateSettings({ category: 'Appearance', settings }).subscribe({
      next: () => {
        alert('Appearance settings saved successfully!');
        this.applyTheme();
        this.isSaving = false;
      },
      error: () => {
        alert('Failed to save settings.');
        this.isSaving = false;
      }
    });
  }

  applyTheme(): void {
    document.documentElement.style.setProperty('--primary-color', this.primaryColor);
    if (this.isDarkModeEnabled) {
      document.body.classList.add('dark-theme');
    } else {
      document.body.classList.remove('dark-theme');
    }
  }
}

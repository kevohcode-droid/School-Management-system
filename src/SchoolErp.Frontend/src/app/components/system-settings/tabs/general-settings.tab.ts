import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsService } from '../../../services/settings.service';

@Component({
  selector: 'app-general-settings-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './general-settings.tab.html',
  styleUrls: ['./general-settings.tab.css']
})
export class GeneralSettingsTab implements OnInit {
  schoolName = '';
  contactEmail = '';
  academicYearStart = '';
  address = '';
  phoneNumber = '';
  isSaving = false;

  constructor(private settingsService: SettingsService) {}

  ngOnInit(): void {
    this.settingsService.getCategory('General').subscribe({
      next: (data: { [key: string]: string }) => {
        this.schoolName = data['SchoolName'] ?? '';
        this.contactEmail = data['ContactEmail'] ?? '';
        this.academicYearStart = data['AcademicYearStart'] ?? '';
        this.address = data['Address'] ?? '';
        this.phoneNumber = data['PhoneNumber'] ?? '';
      },
      error: (err: any) => console.error('Failed to load general settings', err)
    });
  }

  save(): void {
    this.isSaving = true;
    const settings: { [key: string]: string } = {
      SchoolName: this.schoolName,
      ContactEmail: this.contactEmail,
      AcademicYearStart: this.academicYearStart,
      Address: this.address,
      PhoneNumber: this.phoneNumber
    };

    this.settingsService.updateSettings({ category: 'General', settings }).subscribe({
      next: () => {
        alert('General settings saved successfully!');
        this.isSaving = false;
      },
      error: () => {
        alert('Failed to save settings.');
        this.isSaving = false;
      }
    });
  }
}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { GeneralSettingsTab } from './tabs/general-settings.tab';
import { SecuritySettingsTab } from './tabs/security-settings.tab';
import { AppearanceSettingsTab } from './tabs/appearance-settings.tab';

@Component({
  selector: 'app-system-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, GeneralSettingsTab, SecuritySettingsTab, AppearanceSettingsTab],
  templateUrl: './system-settings.component.html',
  styleUrls: ['./system-settings.component.css']
})
export class SystemSettingsComponent {
  activeTab = 'general';
  tabs = [
    { id: 'general', name: 'General Profile' },
    { id: 'security', name: 'Security & Logs' },
    { id: 'appearance', name: 'Layout & Themes' }
  ];

  constructor(private router: Router) {}

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}

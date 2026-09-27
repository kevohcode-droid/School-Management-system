import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <div id="main-content" tabindex="-1" class="min-h-screen bg-slate-50">
      <router-outlet></router-outlet>
    </div>
  `
})
export class AuthLayoutComponent {}
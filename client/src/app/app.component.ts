import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { ToastService, ToastMessage } from './services/toast.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  authService = inject(AuthService);
  toastService = inject(ToastService);
  router = inject(Router);

  currentUser: any = null;
  toasts: (ToastMessage & { id: number })[] = [];
  private toastIdCounter = 0;
  theme: 'dark' | 'light' = 'dark';

  ngOnInit(): void {
    // Load theme setting
    this.theme = (localStorage.getItem('gatekeeper_theme') as 'dark' | 'light') || 'dark';
    this.applyTheme(this.theme);

    // Monitor auth status to show/hide sidebar
    this.authService.currentUser$.subscribe((user: any) => {
      this.currentUser = user;
    });

    // Monitor toast broadcasts
    this.toastService.toast$.subscribe((msg: ToastMessage) => {
      const id = this.toastIdCounter++;
      const toastItem = { ...msg, id };
      this.toasts.push(toastItem);

      // Auto dismiss after 4 seconds
      setTimeout(() => {
        this.toasts = this.toasts.filter(t => t.id !== id);
      }, 4000);
    });
  }

  logout(): void {
    this.authService.logout();
    this.toastService.showSuccess('Logged out successfully.');
    this.router.navigate(['/login']);
  }

  dismissToast(id: number): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
  }

  toggleTheme(): void {
    this.theme = this.theme === 'dark' ? 'light' : 'dark';
    localStorage.setItem('gatekeeper_theme', this.theme);
    this.applyTheme(this.theme);
    this.toastService.showSuccess(`Switched to ${this.theme} theme.`);
  }

  private applyTheme(theme: 'dark' | 'light'): void {
    if (theme === 'light') {
      document.body.classList.add('light-theme');
    } else {
      document.body.classList.remove('light-theme');
    }
  }
}

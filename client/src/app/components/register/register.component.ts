import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  authService = inject(AuthService);
  router = inject(Router);
  toastService = inject(ToastService);

  name = '';
  email = '';
  password = '';
  loading = false;
  errorMessage = '';

  onSubmit(event: Event): void {
    event.preventDefault();
    if (!this.name || !this.email || !this.password) {
      this.errorMessage = 'Please fill out all fields.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.register(this.name, this.email, this.password).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.toastService.showSuccess('Registration successful! Please verify your email OTP code.');
        this.router.navigate(['/verify-otp'], { queryParams: { email: this.email } });
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Registration failed. Try again.';
        this.toastService.showError(this.errorMessage);
      }
    });
  }
}

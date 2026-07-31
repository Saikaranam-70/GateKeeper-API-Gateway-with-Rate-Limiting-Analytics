import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  authService = inject(AuthService);
  router = inject(Router);
  toastService = inject(ToastService);

  step: 1 | 2 = 1;
  email = '';
  otpCode = '';
  newPassword = '';
  confirmPassword = '';
  loading = false;
  errorMessage = '';

  requestResetOtp(event: Event): void {
    event.preventDefault();
    if (!this.email) {
      this.errorMessage = 'Please enter your registered email address.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.forgotPassword(this.email).subscribe({
      next: () => {
        this.loading = false;
        this.step = 2;
        this.toastService.showSuccess('Password reset OTP code sent to your email!');
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to request reset OTP. Try again.';
        this.toastService.showError(this.errorMessage);
      }
    });
  }

  resetPassword(event: Event): void {
    event.preventDefault();
    if (!this.otpCode || this.otpCode.length < 4) {
      this.errorMessage = 'Please enter the 6-digit reset OTP code.';
      return;
    }
    if (!this.newPassword) {
      this.errorMessage = 'Please enter a new password.';
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.resetPassword(this.email, this.otpCode, this.newPassword).subscribe({
      next: () => {
        this.loading = false;
        this.toastService.showSuccess('Password reset successfully! You can now log in with your new password.');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to reset password. Check your OTP code.';
        this.toastService.showError(this.errorMessage);
      }
    });
  }
}

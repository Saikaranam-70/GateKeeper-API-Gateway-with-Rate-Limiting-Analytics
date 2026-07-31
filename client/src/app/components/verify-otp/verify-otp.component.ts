import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './verify-otp.component.html',
  styleUrls: ['./verify-otp.component.css']
})
export class VerifyOtpComponent implements OnInit, OnDestroy {
  authService = inject(AuthService);
  route = inject(ActivatedRoute);
  router = inject(Router);
  toastService = inject(ToastService);

  email = '';
  otpCode = '';
  loading = false;
  resending = false;
  errorMessage = '';

  cooldownSeconds = 0;
  private timer: any = null;

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['email']) {
        this.email = params['email'];
      }
    });
  }

  ngOnDestroy(): void {
    if (this.timer) {
      clearInterval(this.timer);
    }
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    if (!this.email) {
      this.errorMessage = 'Please provide an email address.';
      return;
    }
    if (!this.otpCode || this.otpCode.length < 4) {
      this.errorMessage = 'Please enter the valid OTP code sent to your email.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.verifyOtp(this.email, this.otpCode).subscribe({
      next: () => {
        this.loading = false;
        this.toastService.showSuccess('Email verified successfully! Welcome to GateKeeper.');
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Verification failed. Please check your OTP code.';
        this.toastService.showError(this.errorMessage);
      }
    });
  }

  resendOtp(): void {
    if (!this.email) {
      this.errorMessage = 'Please enter your email address.';
      return;
    }
    if (this.cooldownSeconds > 0 || this.resending) return;

    this.resending = true;
    this.errorMessage = '';

    this.authService.resendOtp(this.email).subscribe({
      next: (res: any) => {
        this.resending = false;
        this.toastService.showSuccess(res.message || 'New OTP code sent to your email!');
        this.startCooldown(60);
      },
      error: (err) => {
        this.resending = false;
        this.errorMessage = err.error?.message || 'Failed to resend OTP. Try again.';
        this.toastService.showError(this.errorMessage);
      }
    });
  }

  private startCooldown(seconds: number): void {
    this.cooldownSeconds = seconds;
    if (this.timer) clearInterval(this.timer);
    this.timer = setInterval(() => {
      this.cooldownSeconds--;
      if (this.cooldownSeconds <= 0) {
        clearInterval(this.timer);
      }
    }, 1000);
  }
}

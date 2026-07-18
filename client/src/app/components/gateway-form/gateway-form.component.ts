import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { GatewayService, Gateway } from '../../services/gateway.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-gateway-form',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './gateway-form.component.html',
  styleUrls: ['./gateway-form.component.css']
})
export class GatewayFormComponent {
  gatewayService = inject(GatewayService);
  router = inject(Router);
  toastService = inject(ToastService);

  name = '';
  description = '';
  targetBaseUrl = '';
  defaultRateLimitPerMin = 100;
  loading = false;
  errorMessage = '';

  onSubmit(event: Event): void {
    event.preventDefault();
    if (!this.name || !this.targetBaseUrl || this.defaultRateLimitPerMin <= 0) {
      this.errorMessage = 'Please complete all required fields with valid values.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const newGateway: Partial<Gateway> = {
      name: this.name,
      description: this.description,
      targetBaseUrl: this.targetBaseUrl,
      defaultRateLimitPerMin: this.defaultRateLimitPerMin,
      status: 'active'
    };

    this.gatewayService.create(newGateway).subscribe({
      next: (res) => {
        this.loading = false;
        this.toastService.showSuccess('Gateway created successfully!');
        if (res && res.id) {
          this.router.navigate(['/gateways', res.id]);
        } else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to create gateway.';
        this.toastService.showError(this.errorMessage);
      }
    });
  }
}

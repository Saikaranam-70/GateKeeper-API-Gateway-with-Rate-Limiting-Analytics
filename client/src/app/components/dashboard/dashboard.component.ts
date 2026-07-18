import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GatewayService, Gateway } from '../../services/gateway.service';
import { AdminService, AdminStats } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  gatewayService = inject(GatewayService);
  adminService = inject(AdminService);
  toastService = inject(ToastService);

  gateways: Gateway[] = [];
  adminStats: AdminStats | null = null;
  isAdmin = false;
  loading = true;

  // Local user metrics
  totalGatewaysCount = 0;
  activeGatewaysCount = 0;
  inactiveGatewaysCount = 0;

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.gatewayService.getAll().subscribe({
      next: (data) => {
        this.gateways = data;
        this.calculateLocalMetrics();
        this.loadAdminStats();
      },
      error: (err) => {
        this.loading = false;
        this.toastService.showError('Failed to load gateways.');
      }
    });
  }

  calculateLocalMetrics(): void {
    this.totalGatewaysCount = this.gateways.length;
    this.activeGatewaysCount = this.gateways.filter(g => g.status === 'active').length;
    this.inactiveGatewaysCount = this.gateways.filter(g => g.status === 'inactive').length;
  }

  loadAdminStats(): void {
    // Attempt to load admin stats. If fails (e.g. 403 Forbidden for non-admin), we fail silently.
    this.adminService.getStats().subscribe({
      next: (stats) => {
        this.adminStats = stats;
        this.isAdmin = true;
        this.loading = false;
      },
      error: () => {
        this.isAdmin = false;
        this.loading = false;
      }
    });
  }

  toggleStatus(gateway: Gateway): void {
    if (!gateway.id) return;
    const newStatus = gateway.status === 'active' ? 'inactive' : 'active';
    this.gatewayService.updateStatus(gateway.id, newStatus).subscribe({
      next: () => {
        gateway.status = newStatus;
        this.calculateLocalMetrics();
        this.toastService.showSuccess(`Gateway status updated to ${newStatus}.`);
      },
      error: () => {
        this.toastService.showError('Failed to update status.');
      }
    });
  }

  simulateTraffic(gateway: Gateway): void {
    if (!gateway.id) return;
    this.toastService.showInfo('Simulating traffic... Please wait.');
    this.gatewayService.simulateTraffic(gateway.id).subscribe({
      next: (res) => {
        this.toastService.showSuccess(`Traffic simulated! Requests: ${res.totalSimulated || 50}`);
      },
      error: () => {
        this.toastService.showError('Traffic simulation failed.');
      }
    });
  }
}

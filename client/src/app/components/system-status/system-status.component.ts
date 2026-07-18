import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService, HealthCheckResult, AdminStats } from '../../services/admin.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-system-status',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './system-status.component.html',
  styleUrls: ['./system-status.component.css']
})
export class SystemStatusComponent implements OnInit {
  adminService = inject(AdminService);
  toastService = inject(ToastService);

  health: HealthCheckResult | null = null;
  adminStats: AdminStats | null = null;
  loading = true;
  isAdmin = false;
  
  // Custom API round-trip latency metric
  apiLatencyMs: number | null = null;

  ngOnInit(): void {
    this.checkSystemStatus();
  }

  checkSystemStatus(): void {
    this.loading = true;
    const startTime = Date.now();

    this.adminService.checkHealth().subscribe({
      next: (res) => {
        this.health = res;
        this.apiLatencyMs = Date.now() - startTime;
        this.loadAdminStats();
      },
      error: () => {
        this.loading = false;
        this.toastService.showError('System status diagnostics call failed.');
      }
    });
  }

  loadAdminStats(): void {
    this.adminService.getStats().subscribe({
      next: (stats) => {
        this.adminStats = stats;
        this.isAdmin = true;
        this.loading = false;
      },
      error: () => {
        // Silent fail for non-admin
        this.isAdmin = false;
        this.loading = false;
      }
    });
  }
}

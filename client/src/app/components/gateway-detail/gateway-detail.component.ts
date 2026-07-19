import { Component, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Chart } from 'chart.js/auto';

import { GatewayService, Gateway, RouteConfig } from '../../services/gateway.service';
import { RateLimitService, RateLimitRule } from '../../services/rate-limit.service';
import { AlertService, AlertRule } from '../../services/alert.service';
import { AnalyticsService, TrafficMetric, LatencyMetric, ErrorMetric, RequestLog } from '../../services/analytics.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-gateway-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './gateway-detail.component.html',
  styleUrls: ['./gateway-detail.component.css']
})
export class GatewayDetailComponent implements OnInit, OnDestroy {
  route = inject(ActivatedRoute);
  router = inject(Router);
  gatewayService = inject(GatewayService);
  rateLimitService = inject(RateLimitService);
  alertService = inject(AlertService);
  analyticsService = inject(AnalyticsService);
  toastService = inject(ToastService);

  gatewayId = '';
  gateway: Gateway | null = null;
  activeTab = 'overview';
  loading = true;

  // Edit Gateway Mode
  isEditingGateway = false;
  editGwName = '';
  editGwDescription = '';
  editGwTargetUrl = '';
  editGwRateLimit = 100;

  // Routes Management
  routes: RouteConfig[] = [];
  routeModalOpen = false;
  editingRoute: RouteConfig | null = null; // null means adding a new route
  routePath = '';
  routeMethods: string[] = ['GET'];
  routeStripPrefix = false;

  // Rate Limits Management
  rateLimits: RateLimitRule[] = [];
  rateLimitModalOpen = false;
  editingRateLimit: RateLimitRule | null = null;
  rlScope: 'global' | 'ip' | 'api-key' = 'global';
  rlRequests = 100;
  rlWindow = 60;
  rlAlgorithm: 'fixed-window' | 'sliding-window' | 'token-bucket' = 'sliding-window';
  rlBurst = 0;
  rlIsActive = true;

  // Alerts Management
  alerts: AlertRule[] = [];
  alertModalOpen = false;
  editingAlert: AlertRule | null = null;
  alName = '';
  alMetric: 'error-rate' | 'latency-p95' | 'request-volume' = 'error-rate';
  alThreshold = 10;
  alUnit: 'percent' | 'ms' | 'count' = 'percent';
  alIsActive = true;

  // Analytics & Logs
  timeWindowHours = 24;
  logsPage = 1;
  logsLimit = 15;
  logsTotal = 0;
  logsTotalPages = 1;
  logsStatusFilter: number | null = null;
  requestLogs: RequestLog[] = [];
  loadingAnalytics = false;
  loadingLogs = false;

  // HTML Canvas references for Chart.js
  @ViewChild('trafficCanvas') trafficCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('latencyCanvas') latencyCanvas!: ElementRef<HTMLCanvasElement>;
  @ViewChild('errorCanvas') errorCanvas!: ElementRef<HTMLCanvasElement>;

  trafficChart: any = null;
  latencyChart: any = null;
  errorChart: any = null;

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.gatewayId = idParam;
      this.loadGateway();
    } else {
      this.toastService.showError('Invalid Gateway URL.');
      this.router.navigate(['/dashboard']);
    }
  }

  ngOnDestroy(): void {
    this.destroyCharts();
  }

  // Load gateway details
  loadGateway(): void {
    this.loading = true;
    this.gatewayService.getById(this.gatewayId).subscribe({
      next: (gw) => {
        this.gateway = gw;
        this.editGwName = gw.name;
        this.editGwDescription = gw.description;
        this.editGwTargetUrl = gw.targetBaseUrl;
        this.editGwRateLimit = gw.defaultRateLimitPerMin;
        this.loading = false;
        
        // Load default tab content
        this.onTabChange(this.activeTab);
      },
      error: () => {
        this.loading = false;
        this.toastService.showError('Failed to fetch gateway details.');
        this.router.navigate(['/dashboard']);
      }
    });
  }

  onTabChange(tab: string): void {
    this.activeTab = tab;
    
    if (tab === 'routes') {
      this.loadRoutes();
    } else if (tab === 'rate-limiting') {
      this.loadRateLimits();
    } else if (tab === 'alerts') {
      this.loadAlerts();
    } else if (tab === 'analytics') {
      this.loadAnalytics();
      this.loadRequestLogs();
    }
  }

  // --- OVERVIEW METADATA CONTROLS ---
  toggleGatewayStatus(): void {
    if (!this.gateway) return;
    const nextStatus = this.gateway.status === 'active' ? 'inactive' : 'active';
    this.gatewayService.updateStatus(this.gatewayId, nextStatus).subscribe({
      next: () => {
        this.gateway!.status = nextStatus;
        this.toastService.showSuccess(`Gateway status toggled to ${nextStatus}.`);
      },
      error: () => this.toastService.showError('Failed to toggle status.')
    });
  }

  enableGatewayEdit(): void {
    this.isEditingGateway = true;
  }

  cancelGatewayEdit(): void {
    this.isEditingGateway = false;
    if (this.gateway) {
      this.editGwName = this.gateway.name;
      this.editGwDescription = this.gateway.description;
      this.editGwTargetUrl = this.gateway.targetBaseUrl;
      this.editGwRateLimit = this.gateway.defaultRateLimitPerMin;
    }
  }

  saveGatewayDetails(): void {
    if (!this.editGwName || !this.editGwTargetUrl || this.editGwRateLimit <= 0) {
      this.toastService.showError('Please complete all required fields.');
      return;
    }

    const payload = {
      name: this.editGwName,
      description: this.editGwDescription,
      targetBaseUrl: this.editGwTargetUrl,
      defaultRateLimitPerMin: this.editGwRateLimit
    };

    this.gatewayService.update(this.gatewayId, payload).subscribe({
      next: (updated) => {
        this.gateway = updated;
        this.isEditingGateway = false;
        this.toastService.showSuccess('Gateway configuration saved.');
      },
      error: () => this.toastService.showError('Failed to save configuration.')
    });
  }

  deleteGateway(): void {
    if (confirm('Are you absolutely sure you want to delete this Gateway? This will delete all its route rules, API keys, rate limit rules, alerts, and live logs permanently.')) {
      this.gatewayService.delete(this.gatewayId).subscribe({
        next: () => {
          this.toastService.showSuccess('Gateway deleted successfully.');
          this.router.navigate(['/dashboard']);
        },
        error: () => this.toastService.showError('Failed to delete gateway.')
      });
    }
  }

  simulateTraffic(): void {
    this.toastService.showInfo('Sending background analytics traffic simulation...');
    this.gatewayService.simulateTraffic(this.gatewayId).subscribe({
      next: (res) => {
        this.toastService.showSuccess(`Traffic simulation complete! Created ${res.totalSimulated || 50} request records.`);
        if (this.activeTab === 'analytics') {
          this.loadAnalytics();
          this.loadRequestLogs();
        }
      },
      error: () => this.toastService.showError('Traffic simulation endpoint failed.')
    });
  }

  // --- ROUTE RULES CRUD ---
  loadRoutes(): void {
    this.gatewayService.getRoutes(this.gatewayId).subscribe({
      next: (data) => this.routes = data,
      error: () => this.toastService.showError('Failed to load gateway routes.')
    });
  }

  openAddRouteModal(): void {
    this.editingRoute = null;
    this.routePath = '';
    this.routeMethods = ['GET'];
    this.routeStripPrefix = false;
    this.routeModalOpen = true;
  }

  openEditRouteModal(route: RouteConfig): void {
    this.editingRoute = route;
    this.routePath = route.path;
    this.routeMethods = [...route.methods];
    this.routeStripPrefix = route.stripPrefix;
    this.routeModalOpen = true;
  }

  toggleMethodSelection(method: string): void {
    const idx = this.routeMethods.indexOf(method);
    if (idx > -1) {
      if (this.routeMethods.length > 1) {
        this.routeMethods.splice(idx, 1);
        this.routeMethods = [...this.routeMethods];
      } else {
        this.toastService.showInfo('A route must support at least one HTTP method.');
      }
    } else {
      this.routeMethods.push(method);
      this.routeMethods = [...this.routeMethods];
    }
  }

  saveRoute(): void {
    if (!this.routePath) {
      this.toastService.showError('Route path template is required.');
      return;
    }

    const payload: RouteConfig = {
      path: this.routePath,
      methods: this.routeMethods,
      stripPrefix: this.routeStripPrefix,
      isActive: true
    };

    if (this.editingRoute && this.editingRoute.id) {
      this.gatewayService.updateRoute(this.gatewayId, this.editingRoute.id, payload).subscribe({
        next: () => {
          this.toastService.showSuccess('Route rule updated.');
          this.routeModalOpen = false;
          this.loadRoutes();
        },
        error: () => this.toastService.showError('Failed to save route.')
      });
    } else {
      this.gatewayService.addRoute(this.gatewayId, payload).subscribe({
        next: () => {
          this.toastService.showSuccess('New route rule added.');
          this.routeModalOpen = false;
          this.loadRoutes();
        },
        error: () => this.toastService.showError('Failed to add route.')
      });
    }
  }

  deleteRoute(routeId?: string): void {
    if (!routeId) return;
    if (confirm('Delete this route config? Upstream proxy requests will no longer map this path.')) {
      this.gatewayService.deleteRoute(this.gatewayId, routeId).subscribe({
        next: () => {
          this.toastService.showSuccess('Route configuration deleted.');
          this.loadRoutes();
        },
        error: () => this.toastService.showError('Failed to delete route.')
      });
    }
  }

  // --- RATE LIMITS CRUD ---
  loadRateLimits(): void {
    this.rateLimitService.list(1, 100, this.gatewayId).subscribe({
      next: (res) => this.rateLimits = res.items,
      error: () => this.toastService.showError('Failed to fetch rate limits.')
    });
  }

  openAddRateLimitModal(): void {
    this.editingRateLimit = null;
    this.rlScope = 'global';
    this.rlRequests = 100;
    this.rlWindow = 60;
    this.rlAlgorithm = 'sliding-window';
    this.rlBurst = 0;
    this.rlIsActive = true;
    this.rateLimitModalOpen = true;
  }

  openEditRateLimitModal(rule: RateLimitRule): void {
    this.editingRateLimit = rule;
    this.rlScope = rule.scope;
    this.rlRequests = rule.requestsPerWindow;
    this.rlWindow = rule.windowSeconds;
    this.rlAlgorithm = rule.algorithm;
    this.rlBurst = rule.burstAllowance || 0;
    this.rlIsActive = rule.isActive;
    this.rateLimitModalOpen = true;
  }

  saveRateLimit(): void {
    const payload: Partial<RateLimitRule> = {
      gatewayId: this.gatewayId,
      scope: this.rlScope,
      requestsPerWindow: this.rlRequests,
      windowSeconds: this.rlWindow,
      algorithm: this.rlAlgorithm,
      burstAllowance: this.rlBurst,
      isActive: this.rlIsActive
    };

    if (this.editingRateLimit && this.editingRateLimit.id) {
      this.rateLimitService.update(this.editingRateLimit.id, payload).subscribe({
        next: () => {
          this.toastService.showSuccess('Rate limiting rule updated.');
          this.rateLimitModalOpen = false;
          this.loadRateLimits();
        },
        error: () => this.toastService.showError('Failed to save rate limit rule.')
      });
    } else {
      this.rateLimitService.create(payload).subscribe({
        next: () => {
          this.toastService.showSuccess('Rate limiting rule created.');
          this.rateLimitModalOpen = false;
          this.loadRateLimits();
        },
        error: () => this.toastService.showError('Failed to create rate limit rule.')
      });
    }
  }

  deleteRateLimit(id?: string): void {
    if (!id) return;
    if (confirm('Delete this rate limiting constraint?')) {
      this.rateLimitService.delete(id).subscribe({
        next: () => {
          this.toastService.showSuccess('Rate limiting rule deleted.');
          this.loadRateLimits();
        },
        error: () => this.toastService.showError('Failed to delete rate limit rule.')
      });
    }
  }

  // --- ALERTS CRUD ---
  loadAlerts(): void {
    this.alertService.list(1, 100, this.gatewayId).subscribe({
      next: (res) => this.alerts = res.items,
      error: () => this.toastService.showError('Failed to load alert rules.')
    });
  }

  openAddAlertModal(): void {
    this.editingAlert = null;
    this.alName = '';
    this.alMetric = 'error-rate';
    this.alThreshold = 10;
    this.alUnit = 'percent';
    this.alIsActive = true;
    this.alertModalOpen = true;
  }

  openEditAlertModal(alert: AlertRule): void {
    this.editingAlert = alert;
    this.alName = alert.name;
    this.alMetric = alert.metricType;
    this.alThreshold = alert.thresholdValue;
    this.alUnit = alert.thresholdUnit;
    this.alIsActive = alert.isActive;
    this.alertModalOpen = true;
  }

  onAlertMetricChange(): void {
    // Automatically match unit options
    if (this.alMetric === 'error-rate') {
      this.alUnit = 'percent';
    } else if (this.alMetric === 'latency-p95') {
      this.alUnit = 'ms';
    } else {
      this.alUnit = 'count';
    }
  }

  saveAlert(): void {
    if (!this.alName) {
      this.toastService.showError('Alert name is required.');
      return;
    }

    const payload: Partial<AlertRule> = {
      gatewayId: this.gatewayId,
      name: this.alName,
      metricType: this.alMetric,
      thresholdValue: this.alThreshold,
      thresholdUnit: this.alUnit,
      isActive: this.alIsActive
    };

    if (this.editingAlert && this.editingAlert.id) {
      this.alertService.update(this.editingAlert.id, payload).subscribe({
        next: () => {
          this.toastService.showSuccess('Alert rule updated.');
          this.alertModalOpen = false;
          this.loadAlerts();
        },
        error: () => this.toastService.showError('Failed to update alert rule.')
      });
    } else {
      this.alertService.create(payload).subscribe({
        next: () => {
          this.toastService.showSuccess('Alert rule created.');
          this.alertModalOpen = false;
          this.loadAlerts();
        },
        error: () => this.toastService.showError('Failed to create alert rule.')
      });
    }
  }

  deleteAlert(id?: string): void {
    if (!id) return;
    if (confirm('Delete this monitoring alert rule?')) {
      this.alertService.delete(id).subscribe({
        next: () => {
          this.toastService.showSuccess('Alert rule deleted.');
          this.loadAlerts();
        },
        error: () => this.toastService.showError('Failed to delete alert rule.')
      });
    }
  }

  // --- LIVE ANALYTICS CHARTS ---
  loadAnalytics(): void {
    this.loadingAnalytics = true;
    
    // Calculate past timestamp based on timeWindowHours
    const fromTime = new Date();
    fromTime.setHours(fromTime.getHours() - this.timeWindowHours);
    const fromStr = fromTime.toISOString();
    const toStr = new Date().toISOString();

    // 1. Fetch Traffic Summary
    this.analyticsService.getTrafficSummary(this.gatewayId, fromStr, toStr, this.timeWindowHours).subscribe({
      next: (trafficData) => {
        // 2. Fetch Latency
        this.analyticsService.getLatency(this.gatewayId, fromStr, toStr, this.timeWindowHours).subscribe({
          next: (latencyData) => {
            // 3. Fetch Error Distribution
            this.analyticsService.getErrors(this.gatewayId, fromStr, toStr, this.timeWindowHours).subscribe({
              next: (errorData) => {
                this.loadingAnalytics = false;
                this.renderCharts(trafficData, latencyData, errorData);
              },
              error: () => {
                this.loadingAnalytics = false;
                this.toastService.showError('Failed to render error analytics.');
              }
            });
          },
          error: () => {
            this.loadingAnalytics = false;
            this.toastService.showError('Failed to fetch latency metrics.');
          }
        });
      },
      error: () => {
        this.loadingAnalytics = false;
        this.toastService.showError('Failed to fetch traffic metrics.');
      }
    });
  }

  renderCharts(traffic: TrafficMetric[], latency: LatencyMetric[], errors: ErrorMetric[]): void {
    // Make sure elements exist before building
    setTimeout(() => {
      this.destroyCharts();

      // Formatter helper for nice chart labels
      const formatTime = (iso: string) => {
        const d = new Date(iso);
        return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      };

      // --- 1. Traffic Chart ---
      if (this.trafficCanvas) {
        const ctx = this.trafficCanvas.nativeElement.getContext('2d');
        if (ctx) {
          this.trafficChart = new Chart(ctx, {
            type: 'line',
            data: {
              labels: traffic.map(t => formatTime(t.timestamp)),
              datasets: [
                {
                  label: 'Success (2xx)',
                  data: traffic.map(t => t.successCount),
                  borderColor: '#10b981',
                  backgroundColor: 'rgba(16, 185, 129, 0.05)',
                  fill: true,
                  tension: 0.3
                },
                {
                  label: 'Rate Limited (429)',
                  data: traffic.map(t => t.rateLimitedCount),
                  borderColor: '#fbbf24',
                  backgroundColor: 'rgba(251, 191, 36, 0.05)',
                  fill: true,
                  tension: 0.3
                },
                {
                  label: 'Errors (5xx)',
                  data: traffic.map(t => t.errorCount),
                  borderColor: '#ef4444',
                  backgroundColor: 'rgba(239, 68, 68, 0.05)',
                  fill: true,
                  tension: 0.3
                }
              ]
            },
            options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: { labels: { color: '#9ca3af', font: { family: 'Plus Jakarta Sans' } } }
              },
              scales: {
                x: { ticks: { color: '#6b7280' }, grid: { color: 'rgba(255,255,255,0.03)' } },
                y: { ticks: { color: '#6b7280' }, grid: { color: 'rgba(255,255,255,0.03)' } }
              }
            }
          });
        }
      }

      // --- 2. Latency Chart ---
      if (this.latencyCanvas) {
        const ctx = this.latencyCanvas.nativeElement.getContext('2d');
        if (ctx) {
          this.latencyChart = new Chart(ctx, {
            type: 'line',
            data: {
              labels: latency.map(l => formatTime(l.timestamp)),
              datasets: [
                {
                  label: 'Avg Latency (ms)',
                  data: latency.map(l => l.avgLatencyMs),
                  borderColor: '#6366f1',
                  backgroundColor: 'transparent',
                  tension: 0.3
                },
                {
                  label: 'P95 Latency (ms)',
                  data: latency.map(l => l.p95LatencyMs),
                  borderColor: '#06b6d4',
                  backgroundColor: 'transparent',
                  tension: 0.3
                }
              ]
            },
            options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: { labels: { color: '#9ca3af', font: { family: 'Plus Jakarta Sans' } } }
              },
              scales: {
                x: { ticks: { color: '#6b7280' }, grid: { color: 'rgba(255,255,255,0.03)' } },
                y: { ticks: { color: '#6b7280' }, grid: { color: 'rgba(255,255,255,0.03)' } }
              }
            }
          });
        }
      }

      // --- 3. Error Code Distribution Chart ---
      if (this.errorCanvas) {
        const ctx = this.errorCanvas.nativeElement.getContext('2d');
        if (ctx) {
          const colorsMap: { [key: number]: string } = {
            400: '#fca5a5',
            401: '#f87171',
            403: '#ef4444',
            404: '#dc2626',
            429: '#f59e0b',
            500: '#991b1b'
          };
          const bgColors = errors.map(e => colorsMap[e.statusCode] || '#6b7280');

          this.errorChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
              labels: errors.map(e => `HTTP ${e.statusCode}`),
              datasets: [{
                data: errors.map(e => e.count),
                backgroundColor: bgColors,
                borderWidth: 1,
                borderColor: 'rgba(255,255,255,0.08)'
              }]
            },
            options: {
              responsive: true,
              maintainAspectRatio: false,
              plugins: {
                legend: { position: 'right', labels: { color: '#9ca3af', font: { family: 'Plus Jakarta Sans' } } }
              }
            }
          });
        }
      }

    }, 50);
  }

  destroyCharts(): void {
    if (this.trafficChart) {
      this.trafficChart.destroy();
      this.trafficChart = null;
    }
    if (this.latencyChart) {
      this.latencyChart.destroy();
      this.latencyChart = null;
    }
    if (this.errorChart) {
      this.errorChart.destroy();
      this.errorChart = null;
    }
  }

  // --- HISTORICAL LOGS ---
  loadRequestLogs(): void {
    this.loadingLogs = true;
    const fromTime = new Date();
    fromTime.setHours(fromTime.getHours() - this.timeWindowHours);
    const fromStr = fromTime.toISOString();
    const toStr = new Date().toISOString();

    this.analyticsService.getRequestLogs(
      this.gatewayId,
      fromStr,
      toStr,
      this.logsStatusFilter,
      this.logsPage,
      this.logsLimit
    ).subscribe({
      next: (res) => {
        this.requestLogs = res.items;
        this.logsTotal = res.totalCount;
        this.logsTotalPages = res.totalPages;
        this.loadingLogs = false;
      },
      error: () => {
        this.loadingLogs = false;
        this.toastService.showError('Failed to retrieve HTTP request logs.');
      }
    });
  }

  filterLogsByStatus(code: number | null): void {
    this.logsStatusFilter = code;
    this.logsPage = 1;
    this.loadRequestLogs();
  }

  changeLogsPage(page: number): void {
    if (page >= 1 && page <= this.logsTotalPages) {
      this.logsPage = page;
      this.loadRequestLogs();
    }
  }
}

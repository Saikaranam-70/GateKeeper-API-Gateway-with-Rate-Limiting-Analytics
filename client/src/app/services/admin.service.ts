import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { CacheService } from './cache.service';

export interface HealthCheckResult {
  status: string;
  checks: {
    sql: { status: string; error?: string };
    redis: { status: string; pingMs?: number; error?: string };
    yarp: { status: string; error?: string };
  };
}

export interface AdminStats {
  totalAlerts: number;
  activeAlerts: number;
  inactiveAlerts: number;
  totalGateways: number;
  totalUsers: number;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private cacheService = inject(CacheService);

  private adminUrl = 'http://localhost:5041/api/admin';
  private healthUrl = 'http://localhost:5041/api/health';

  /**
   * Get Platform Stats (cached for 10 seconds).
   */
  getStats(): Observable<AdminStats> {
    const url = `${this.adminUrl}/stats`;
    const cached = this.cacheService.get(url);
    if (cached) {
      return of(cached);
    }
    return this.http.get<AdminStats>(url).pipe(
      tap((data: AdminStats) => this.cacheService.set(url, data, 10))
    );
  }

  /**
   * Get System Health Check (cached for 5 seconds).
   */
  checkHealth(): Observable<HealthCheckResult> {
    const cached = this.cacheService.get(this.healthUrl);
    if (cached) {
      return of(cached);
    }
    return this.http.get<HealthCheckResult>(this.healthUrl).pipe(
      tap((data: HealthCheckResult) => this.cacheService.set(this.healthUrl, data, 5))
    );
  }
}

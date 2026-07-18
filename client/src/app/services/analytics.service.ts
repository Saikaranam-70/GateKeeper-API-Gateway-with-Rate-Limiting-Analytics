import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { CacheService } from './cache.service';
import { PaginatedResult } from './rate-limit.service';

export interface TrafficMetric {
  timestamp: string;
  totalRequests: number;
  successCount: number;
  rateLimitedCount: number;
  errorCount: number;
}

export interface LatencyMetric {
  timestamp: string;
  avgLatencyMs: number;
  p95LatencyMs: number;
  p99LatencyMs: number;
}

export interface ErrorMetric {
  timestamp: string;
  statusCode: number;
  count: number;
}

export interface RequestLog {
  id: string;
  gatewayId: string;
  apiKeyId?: string;
  apiKeyLabel?: string;
  clientIp: string;
  requestPath: string;
  httpMethod: string;
  statusCode: number;
  latencyMs: number;
  errorMessage?: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private http = inject(HttpClient);
  private cacheService = inject(CacheService);

  private apiUrl = 'http://localhost:5041/api/analytics';

  /**
   * Fetch Traffic Summary (cached for 5 seconds).
   */
  getTrafficSummary(gatewayId: string, from?: string, to?: string, windowHours: number = 24): Observable<TrafficMetric[]> {
    let url = `${this.apiUrl}/${gatewayId}?windowHours=${windowHours}`;
    if (from) url += `&from=${encodeURIComponent(from)}`;
    if (to) url += `&to=${encodeURIComponent(to)}`;

    const cached = this.cacheService.get(url);
    if (cached) return of(cached);

    return this.http.get<TrafficMetric[]>(url).pipe(
      tap((data: TrafficMetric[]) => this.cacheService.set(url, data, 5))
    );
  }

  /**
   * Fetch Latency Analytics (cached for 5 seconds).
   */
  getLatency(gatewayId: string, from?: string, to?: string, windowHours: number = 24): Observable<LatencyMetric[]> {
    let url = `${this.apiUrl}/${gatewayId}/latency?windowHours=${windowHours}`;
    if (from) url += `&from=${encodeURIComponent(from)}`;
    if (to) url += `&to=${encodeURIComponent(to)}`;

    const cached = this.cacheService.get(url);
    if (cached) return of(cached);

    return this.http.get<LatencyMetric[]>(url).pipe(
      tap((data: LatencyMetric[]) => this.cacheService.set(url, data, 5))
    );
  }

  /**
   * Fetch Error Analytics (cached for 5 seconds).
   */
  getErrors(gatewayId: string, from?: string, to?: string, windowHours: number = 24): Observable<ErrorMetric[]> {
    let url = `${this.apiUrl}/${gatewayId}/errors?windowHours=${windowHours}`;
    if (from) url += `&from=${encodeURIComponent(from)}`;
    if (to) url += `&to=${encodeURIComponent(to)}`;

    const cached = this.cacheService.get(url);
    if (cached) return of(cached);

    return this.http.get<ErrorMetric[]>(url).pipe(
      tap((data: ErrorMetric[]) => this.cacheService.set(url, data, 5))
    );
  }

  /**
   * Fetch Request Logs (cached for 5 seconds).
   */
  getRequestLogs(
    gatewayId: string,
    from?: string,
    to?: string,
    statusCode?: number | null,
    page: number = 1,
    limit: number = 50
  ): Observable<PaginatedResult<RequestLog>> {
    let url = `${this.apiUrl}/${gatewayId}/requests?page=${page}&limit=${limit}`;
    if (from) url += `&from=${encodeURIComponent(from)}`;
    if (to) url += `&to=${encodeURIComponent(to)}`;
    if (statusCode !== undefined && statusCode !== null) url += `&statusCode=${statusCode}`;

    const cached = this.cacheService.get(url);
    if (cached) return of(cached);

    return this.http.get<PaginatedResult<RequestLog>>(url).pipe(
      tap((data: PaginatedResult<RequestLog>) => this.cacheService.set(url, data, 5))
    );
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { CacheService } from './cache.service';
import { PaginatedResult } from './rate-limit.service';

export interface AlertRule {
  id?: string;
  gatewayId: string;
  gatewayName?: string;
  name: string;
  metricType: 'error-rate' | 'latency-p95' | 'request-volume';
  thresholdValue: number;
  thresholdUnit: 'percent' | 'ms' | 'count';
  isActive: boolean;
  lastTriggeredAt?: string | null;
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AlertService {
  private http = inject(HttpClient);
  private cacheService = inject(CacheService);

  private apiUrl = 'http://localhost:5041/api/alerts';

  /**
   * Helper to invalidate cache keys.
   */
  private invalidateCache(): void {
    this.cacheService.invalidate(this.apiUrl);
  }

  /**
   * List all alert rules (cached for 15 seconds).
   */
  list(page: number = 1, limit: number = 50, gatewayId?: string): Observable<PaginatedResult<AlertRule>> {
    let url = `${this.apiUrl}?page=${page}&limit=${limit}`;
    if (gatewayId) {
      url += `&gatewayId=${gatewayId}`;
    }

    const cached = this.cacheService.get(url);
    if (cached) {
      return of(cached);
    }

    return this.http.get<PaginatedResult<AlertRule>>(url).pipe(
      tap((data: PaginatedResult<AlertRule>) => this.cacheService.set(url, data, 15))
    );
  }

  /**
   * Create an alert rule.
   */
  create(rule: Partial<AlertRule>): Observable<AlertRule> {
    return this.http.post<AlertRule>(this.apiUrl, rule).pipe(
      tap(() => this.invalidateCache())
    );
  }

  /**
   * Update an alert rule.
   */
  update(id: string, rule: Partial<AlertRule>): Observable<AlertRule> {
    return this.http.put<AlertRule>(`${this.apiUrl}/${id}`, rule).pipe(
      tap(() => this.invalidateCache())
    );
  }

  /**
   * Delete an alert rule.
   */
  delete(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.invalidateCache())
    );
  }
}

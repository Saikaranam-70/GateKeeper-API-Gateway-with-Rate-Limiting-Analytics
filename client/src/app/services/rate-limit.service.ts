import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { CacheService } from './cache.service';

export interface RateLimitRule {
  id?: string;
  gatewayId: string;
  gatewayName?: string;
  scope: 'global' | 'ip' | 'api-key';
  requestsPerWindow: number;
  windowSeconds: number;
  algorithm: 'fixed-window' | 'sliding-window' | 'token-bucket';
  burstAllowance?: number;
  isActive: boolean;
  createdAt?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  page: number;
  limit: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class RateLimitService {
  private http = inject(HttpClient);
  private cacheService = inject(CacheService);

  private apiUrl = 'http://localhost:5041/api/rate-limits';

  /**
   * Helper to invalidate cache keys.
   */
  private invalidateCache(): void {
    this.cacheService.invalidate(this.apiUrl);
  }

  /**
   * Get all rate limits (cached for 15 seconds).
   */
  list(page: number = 1, limit: number = 50, gatewayId?: string): Observable<PaginatedResult<RateLimitRule>> {
    let url = `${this.apiUrl}?page=${page}&limit=${limit}`;
    if (gatewayId) {
      url += `&gatewayId=${gatewayId}`;
    }

    const cached = this.cacheService.get(url);
    if (cached) {
      return of(cached);
    }

    return this.http.get<PaginatedResult<RateLimitRule>>(url).pipe(
      tap((data: PaginatedResult<RateLimitRule>) => this.cacheService.set(url, data, 15))
    );
  }

  /**
   * Create a rule.
   */
  create(rule: Partial<RateLimitRule>): Observable<RateLimitRule> {
    return this.http.post<RateLimitRule>(this.apiUrl, rule).pipe(
      tap(() => this.invalidateCache())
    );
  }

  /**
   * Update a rule.
   */
  update(id: string, rule: Partial<RateLimitRule>): Observable<RateLimitRule> {
    return this.http.put<RateLimitRule>(`${this.apiUrl}/${id}`, rule).pipe(
      tap(() => this.invalidateCache())
    );
  }

  /**
   * Delete a rule.
   */
  delete(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.invalidateCache())
    );
  }
}

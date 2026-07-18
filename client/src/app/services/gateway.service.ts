import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { CacheService } from './cache.service';

export interface RouteConfig {
  id?: string;
  gatewayId?: string;
  path: string;
  methods: string[];
  stripPrefix: boolean;
  isActive?: boolean;
}

export interface Gateway {
  id?: string;
  name: string;
  description: string;
  targetBaseUrl: string;
  defaultRateLimitPerMin: number;
  status: 'active' | 'inactive';
  createdAt?: string;
  routes?: RouteConfig[];
}

@Injectable({
  providedIn: 'root'
})
export class GatewayService {
  private http = inject(HttpClient);
  private cacheService = inject(CacheService);

  private apiUrl = 'http://localhost:5041/api/gateway';

  /**
   * Helper to invalidate all cache keys related to gateways and analytics.
   */
  private invalidateCache(gatewayId?: string): void {
    const patterns = ['/api/gateway'];
    if (gatewayId) {
      patterns.push(`/api/analytics/${gatewayId}`);
    } else {
      patterns.push('/api/analytics');
    }
    this.cacheService.invalidateMultiple(patterns);
  }

  /**
   * Get all gateways (cached for 15 seconds).
   */
  getAll(): Observable<Gateway[]> {
    const cacheKey = this.apiUrl;
    const cached = this.cacheService.get(cacheKey);
    if (cached) {
      return of(cached);
    }
    return this.http.get<Gateway[]>(this.apiUrl).pipe(
      tap((data: Gateway[]) => this.cacheService.set(cacheKey, data, 15))
    );
  }

  /**
   * Get gateway by ID (cached for 15 seconds).
   */
  getById(id: string): Observable<Gateway> {
    const cacheKey = `${this.apiUrl}/${id}`;
    const cached = this.cacheService.get(cacheKey);
    if (cached) {
      return of(cached);
    }
    return this.http.get<Gateway>(cacheKey).pipe(
      tap((data: Gateway) => this.cacheService.set(cacheKey, data, 15))
    );
  }

  /**
   * Create a new gateway (invalidates gateways cache).
   */
  create(gateway: Partial<Gateway>): Observable<Gateway> {
    return this.http.post<Gateway>(this.apiUrl, gateway).pipe(
      tap(() => this.invalidateCache())
    );
  }

  /**
   * Update gateway details (invalidates caches).
   */
  update(id: string, gateway: Partial<Gateway>): Observable<Gateway> {
    return this.http.put<Gateway>(`${this.apiUrl}/${id}`, gateway).pipe(
      tap(() => this.invalidateCache(id))
    );
  }

  /**
   * Delete a gateway (invalidates caches).
   */
  delete(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.invalidateCache(id))
    );
  }

  /**
   * Toggle status active/inactive.
   */
  updateStatus(id: string, status: 'active' | 'inactive'): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/status`, { status }).pipe(
      tap(() => this.invalidateCache(id))
    );
  }

  /**
   * Simulate Traffic for testing.
   */
  simulateTraffic(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/simulate-traffic`, {}).pipe(
      tap(() => {
        // Invalidate analytics and request logs for this gateway
        this.cacheService.invalidate(`/api/analytics/${id}`);
      })
    );
  }

  /**
   * Get Gateway routes (cached for 15 seconds).
   */
  getRoutes(gatewayId: string): Observable<RouteConfig[]> {
    const cacheKey = `${this.apiUrl}/${gatewayId}/routes`;
    const cached = this.cacheService.get(cacheKey);
    if (cached) {
      return of(cached);
    }
    return this.http.get<RouteConfig[]>(cacheKey).pipe(
      tap((data: RouteConfig[]) => this.cacheService.set(cacheKey, data, 15))
    );
  }

  /**
   * Add Route to Gateway.
   */
  addRoute(gatewayId: string, route: RouteConfig): Observable<RouteConfig> {
    return this.http.post<RouteConfig>(`${this.apiUrl}/${gatewayId}/routes`, route).pipe(
      tap(() => this.invalidateCache(gatewayId))
    );
  }

  /**
   * Update Route.
   */
  updateRoute(gatewayId: string, routeId: string, route: RouteConfig): Observable<RouteConfig> {
    return this.http.put<RouteConfig>(`${this.apiUrl}/${gatewayId}/routes/${routeId}`, route).pipe(
      tap(() => this.invalidateCache(gatewayId))
    );
  }

  /**
   * Delete Route.
   */
  deleteRoute(gatewayId: string, routeId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${gatewayId}/routes/${routeId}`).pipe(
      tap(() => this.invalidateCache(gatewayId))
    );
  }
}

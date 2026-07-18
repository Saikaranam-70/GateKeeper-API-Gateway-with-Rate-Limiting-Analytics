import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap } from 'rxjs';
import { CacheService } from './cache.service';

export interface ApiKey {
  id?: string;
  gatewayId: string;
  gatewayName?: string;
  label: string;
  maskedKey?: string;
  rawApiKey?: string; // Only returned once on creation!
  expiresAt: string | null;
  createdAt?: string;
  isRevoked?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ApiKeyService {
  private http = inject(HttpClient);
  private cacheService = inject(CacheService);

  private apiUrl = 'http://localhost:5041/api/api-keys';

  /**
   * Get all API keys (cached for 15 seconds).
   */
  getAll(): Observable<ApiKey[]> {
    const cached = this.cacheService.get(this.apiUrl);
    if (cached) {
      return of(cached);
    }
    return this.http.get<ApiKey[]>(this.apiUrl).pipe(
      tap(data => this.cacheService.set(this.apiUrl, data, 15))
    );
  }

  /**
   * Generate a new API key.
   */
  generate(gatewayId: string, label: string, expiresAt: string | null): Observable<ApiKey> {
    return this.http.post<ApiKey>(this.apiUrl, { gatewayId, label, expiresAt }).pipe(
      tap(() => this.cacheService.invalidate(this.apiUrl))
    );
  }

  /**
   * Revoke an API key.
   */
  revoke(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.cacheService.invalidate(this.apiUrl))
    );
  }
}

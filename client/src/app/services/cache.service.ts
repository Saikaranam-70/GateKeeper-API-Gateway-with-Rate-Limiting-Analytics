import { Injectable } from '@angular/core';

interface CacheEntry {
  data: any;
  expiry: number;
}

@Injectable({
  providedIn: 'root'
})
export class CacheService {
  private cache = new Map<string, CacheEntry>();

  /**
   * Get cached data if it exists and is not expired.
   */
  get(key: string): any | null {
    const entry = this.cache.get(key);
    if (!entry) {
      return null;
    }

    if (Date.now() > entry.expiry) {
      this.cache.delete(key);
      return null;
    }

    return entry.data;
  }

  /**
   * Set cached data with a specified TTL (in seconds).
   */
  set(key: string, data: any, ttlSeconds: number = 30): void {
    const expiry = Date.now() + (ttlSeconds * 1000);
    this.cache.set(key, { data, expiry });
  }

  /**
   * Invalidate cache keys matching a pattern.
   * Can accept a string prefix or a regular expression.
   */
  invalidate(pattern: string | RegExp): void {
    for (const key of this.cache.keys()) {
      if (typeof pattern === 'string') {
        if (key.includes(pattern)) {
          this.cache.delete(key);
        }
      } else if (pattern.test(key)) {
        this.cache.delete(key);
      }
    }
  }

  /**
   * Invalidate caching entries for multiple patterns at once.
   */
  invalidateMultiple(patterns: (string | RegExp)[]): void {
    patterns.forEach(p => this.invalidate(p));
  }

  /**
   * Clear the entire cache (useful on logout).
   */
  clear(): void {
    this.cache.clear();
  }
}

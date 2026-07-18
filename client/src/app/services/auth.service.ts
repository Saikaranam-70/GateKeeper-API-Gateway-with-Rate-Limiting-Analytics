import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, map, catchError, of } from 'rxjs';
import { CacheService } from './cache.service';

export interface User {
  id: string;
  name: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private cacheService = inject(CacheService);

  private apiUrl = 'http://localhost:5041/api';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    this.loadCurrentUser();
  }

  /**
   * Helper to check if token exists.
   */
  getToken(): string | null {
    return localStorage.getItem('gatekeeper_token');
  }

  /**
   * Save token to localStorage.
   */
  setToken(token: string): void {
    localStorage.setItem('gatekeeper_token', token);
  }

  /**
   * Remove token.
   */
  removeToken(): void {
    localStorage.removeItem('gatekeeper_token');
  }

  /**
   * Check if user is authenticated.
   */
  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  /**
   * Register a new user.
   */
  register(name: string, email: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/user/register`, { name, email, password });
  }

  /**
   * Log in user and save JWT token.
   */
  login(email: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/user/login`, { email, password }).pipe(
      tap((res: any) => {
        if (res && res.token) {
          this.setToken(res.token);
          this.loadCurrentUser();
        }
      })
    );
  }

  /**
   * Load profile of current user.
   */
  loadCurrentUser(): void {
    if (!this.isAuthenticated()) {
      this.currentUserSubject.next(null);
      return;
    }

    // Cache user profile for 60 seconds
    const cacheKey = `${this.apiUrl}/auth/me`;
    const cachedUser = this.cacheService.get(cacheKey);

    if (cachedUser) {
      this.currentUserSubject.next(cachedUser);
      return;
    }

    this.http.get<User>(`${this.apiUrl}/auth/me`).subscribe({
      next: (user: User) => {
        this.cacheService.set(cacheKey, user, 60);
        this.currentUserSubject.next(user);
      },
      error: () => {
        this.logout();
      }
    });
  }

  /**
   * Log out user, remove token, clear cache, and reset user subject.
   */
  logout(): void {
    this.removeToken();
    this.cacheService.clear();
    this.currentUserSubject.next(null);
  }
}

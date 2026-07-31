import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { CacheService } from './cache.service';
import { environment } from '../../environments/environment';

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

  private apiUrl = environment.apiUrl;
  
  // Initialize synchronously from localStorage to fix dashboard refresh issue
  private currentUserSubject = new BehaviorSubject<User | null>(this.getStoredUser());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    // If token exists, refresh user profile in background
    if (this.isAuthenticated()) {
      this.loadCurrentUser();
    }
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
   * Save user to localStorage.
   */
  setStoredUser(user: User): void {
    localStorage.setItem('gatekeeper_user', JSON.stringify(user));
  }

  /**
   * Retrieve user from localStorage.
   */
  getStoredUser(): User | null {
    const raw = localStorage.getItem('gatekeeper_user');
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  /**
   * Remove token and stored user.
   */
  removeAuthData(): void {
    localStorage.removeItem('gatekeeper_token');
    localStorage.removeItem('gatekeeper_user');
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
   * Log in user and save JWT token & user.
   */
  login(email: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/user/login`, { email, password }).pipe(
      tap((res: any) => {
        if (res && res.token) {
          this.setToken(res.token);
          if (res.user) {
            this.setStoredUser(res.user);
            this.currentUserSubject.next(res.user);
          } else {
            this.loadCurrentUser();
          }
        }
      })
    );
  }

  /**
   * Verify email OTP code.
   */
  verifyOtp(email: string, otpCode: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/user/verify-otp`, { email, otpCode }).pipe(
      tap((res: any) => {
        if (res && res.token) {
          this.setToken(res.token);
          if (res.user) {
            this.setStoredUser(res.user);
            this.currentUserSubject.next(res.user);
          }
        }
      })
    );
  }

  /**
   * Resend verification OTP code.
   */
  resendOtp(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/user/resend-otp`, { email });
  }

  /**
   * Request password reset OTP code.
   */
  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/user/forgot-password`, { email });
  }

  /**
   * Reset password with OTP code and new password.
   */
  resetPassword(email: string, otpCode: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/user/reset-password`, { email, otpCode, newPassword });
  }

  /**
   * Load profile of current user.
   */
  loadCurrentUser(): void {
    if (!this.isAuthenticated()) {
      this.currentUserSubject.next(null);
      return;
    }

    const cacheKey = `${this.apiUrl}/auth/me`;

    this.http.get<User>(`${this.apiUrl}/auth/me`).subscribe({
      next: (user: User) => {
        this.cacheService.set(cacheKey, user, 60);
        this.setStoredUser(user);
        this.currentUserSubject.next(user);
      },
      error: (err) => {
        // ONLY log out if 401 Unauthorized, to prevent accidental logout on network or 500 errors
        if (err && err.status === 401) {
          this.logout();
        }
      }
    });
  }

  /**
   * Log out user, remove token, clear cache, and reset user subject.
   */
  logout(): void {
    this.removeAuthData();
    this.cacheService.clear();
    this.currentUserSubject.next(null);
  }
}

import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CurrentUser {
  id:          string;
  email:       string;
  role:        'Admin' | 'Leader' | 'Advisor';
  firstName:   string;
  lastName:    string;
  advisorCode: string | null;
  token:       string;
  expiresAt:   string;
}

interface LoginResponse {
  token:       string;
  role:        string;
  userId:      string;
  email:       string;
  firstName:   string;
  lastName:    string;
  advisorCode: string | null;
  expiresAt:   string;
}

const STORAGE_KEY = 'insuratech_user';
const API_BASE    = `${environment.apiUrl}/api/v1`;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http   = inject(HttpClient);
  private router = inject(Router);

  private _currentUser = signal<CurrentUser | null>(this._loadFromStorage());

  readonly currentUser     = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
  readonly token           = computed(() => this._currentUser()?.token ?? null);
  readonly role            = computed(() => this._currentUser()?.role ?? null);

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${API_BASE}/auth/login`, { email, password })
      .pipe(
        tap(res => {
          const user: CurrentUser = {
            id:          res.userId,
            email:       res.email,
            role:        res.role as CurrentUser['role'],
            firstName:   res.firstName,
            lastName:    res.lastName,
            advisorCode: res.advisorCode,
            token:       res.token,
            expiresAt:   res.expiresAt,
          };
          this._currentUser.set(user);
          localStorage.setItem(STORAGE_KEY, JSON.stringify(user));
        })
      );
  }

  logout(): void {
    this._currentUser.set(null);
    localStorage.removeItem(STORAGE_KEY);
    this.router.navigate(['/login']);
  }

  private _loadFromStorage(): CurrentUser | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const user = JSON.parse(raw) as CurrentUser;
      // Si el token ya expiró, limpiar
      if (new Date(user.expiresAt) < new Date()) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return user;
    } catch {
      return null;
    }
  }
}

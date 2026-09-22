import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { API_BASE_URL } from '../config/app-config';
import {
  AuthResult,
  LoginRequest,
  RegisterDriverRequest,
  RegisterRiderRequest,
  UserRole
} from '../models/auth.models';

const STORAGE_KEY = 'rideshare.auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly authState = signal<AuthResult | null>(this.readFromStorage());

  readonly currentUser = computed(() => this.authState());
  readonly isAuthenticated = computed(() => this.authState() !== null);
  readonly role = computed<UserRole | null>(() => this.authState()?.roles[0] ?? null);

  constructor(private readonly http: HttpClient, private readonly router: Router) {}

  get token(): string | null {
    return this.authState()?.token ?? null;
  }

  async registerRider(request: RegisterRiderRequest): Promise<void> {
    const result = await firstValueFrom(this.http.post<AuthResult>(`${API_BASE_URL}/auth/register/rider`, request));
    this.setSession(result);
  }

  async registerDriver(request: RegisterDriverRequest): Promise<void> {
    const result = await firstValueFrom(this.http.post<AuthResult>(`${API_BASE_URL}/auth/register/driver`, request));
    this.setSession(result);
  }

  async login(request: LoginRequest): Promise<void> {
    const result = await firstValueFrom(this.http.post<AuthResult>(`${API_BASE_URL}/auth/login`, request));
    this.setSession(result);
  }

  logout(): void {
    this.authState.set(null);
    localStorage.removeItem(STORAGE_KEY);
    this.router.navigateByUrl('/login');
  }

  private setSession(result: AuthResult): void {
    this.authState.set(result);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(result));
  }

  private readFromStorage(): AuthResult | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as AuthResult;
    } catch {
      return null;
    }
  }
}

import { Injectable, computed, signal, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { tap } from "rxjs";

export type UserRole = "admin" | "user";

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  role: UserRole;
}

interface AuthResponse {
  token: string;
  user: AuthUser;
}

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly tokenKey = "adminTool.token";
  private readonly userKey = "adminTool.user";
  private readonly baseUrl = "/api";

  private readonly userSignal = signal<AuthUser | null>(null);
  readonly user = computed(() => this.userSignal());
  readonly isAuthenticated = computed(() => !!this.userSignal());
  readonly isAdmin = computed(() => this.userSignal()?.role === "admin");


  constructor() {
    const savedUser = window.localStorage.getItem(this.userKey);
    const savedToken = window.localStorage.getItem(this.tokenKey);

    if (savedUser && savedToken) {
      this.userSignal.set(JSON.parse(savedUser));
      return;
    }

    window.localStorage.removeItem(this.userKey);
    window.localStorage.removeItem(this.tokenKey);
  }

  getToken(): string | null {
    return window.localStorage.getItem(this.tokenKey);
  }

  login(email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/login`, { email, password })
      .pipe(tap((response) => this.persistAuth(response)));
  }

  register(name: string, email: string, password: string) {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/register`, { name, email, password })
      .pipe(tap((response) => this.persistAuth(response)));
  }

  logout() {
    window.localStorage.removeItem(this.tokenKey);
    window.localStorage.removeItem(this.userKey);
    this.userSignal.set(null);
  }

  private persistAuth(response: AuthResponse) {
    window.localStorage.setItem(this.tokenKey, response.token);
    window.localStorage.setItem(this.userKey, JSON.stringify(response.user));
    this.userSignal.set(response.user);
  }
}

import { Injectable, computed, signal } from "@angular/core";
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
  private readonly tokenKey = "adminTool.token";
  private readonly userKey = "adminTool.user";
  private readonly baseUrl = "/api";

  private readonly userSignal = signal<AuthUser | null>(null);
  readonly user = computed(() => this.userSignal());
  readonly isAuthenticated = computed(() => !!this.userSignal());
  readonly isAdmin = computed(() => this.userSignal()?.role === "admin");

  constructor(private readonly http: HttpClient) {
    const savedUser = localStorage.getItem(this.userKey);
    const savedToken = localStorage.getItem(this.tokenKey);

    if (savedUser && savedToken) {
      this.userSignal.set(JSON.parse(savedUser));
      return;
    }

    localStorage.removeItem(this.userKey);
    localStorage.removeItem(this.tokenKey);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
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
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.userSignal.set(null);
  }

  private persistAuth(response: AuthResponse) {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.userKey, JSON.stringify(response.user));
    this.userSignal.set(response.user);
  }
}

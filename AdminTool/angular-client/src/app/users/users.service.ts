import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { AuthUser } from "../core/auth.service";

export interface UserItem extends AuthUser {
  createdAt: string;
  updatedAt: string;
}

interface ListResponse {
  items: UserItem[];
}

interface ItemResponse {
  item: UserItem;
}

@Injectable({ providedIn: "root" })
export class UsersService {
  private readonly baseUrl = "/api";

  constructor(private readonly http: HttpClient) {}

  list() {
    return this.http.get<ListResponse>(`${this.baseUrl}/users`);
  }

  getById(id: string) {
    return this.http.get<ItemResponse>(`${this.baseUrl}/users/${id}`);
  }

  create(payload: {
    name: string;
    email: string;
    role: "admin" | "user";
    password: string;
  }) {
    return this.http.post<ItemResponse>(`${this.baseUrl}/users`, payload);
  }

  update(id: string, payload: {
    name?: string;
    email?: string;
    role?: "admin" | "user";
    password?: string;
  }) {
    return this.http.put<ItemResponse>(`${this.baseUrl}/users/${id}`, payload);
  }

  remove(id: string) {
    return this.http.delete<{ ok: boolean }>(`${this.baseUrl}/users/${id}`);
  }
}

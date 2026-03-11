import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { CreateUserPayload, UpdateUserPayload, UserAuditLogItem, UserItem } from "../../features/users/domain/user.models";

interface ListResponse {
  items: UserItem[];
}

interface ItemResponse {
  item: UserItem;
}

interface AuditLogListResponse {
  items: UserAuditLogItem[];
}

@Injectable({ providedIn: "root" })
export class UsersService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = "/api";


  constructor() {}

  list(sort?: Array<{ field: string; direction: "asc" | "desc" }>, query?: string) {
    let params = new HttpParams();

    for (const item of sort ?? []) {
      params = params.append("sort", `${item.field}:${item.direction}`);
    }

    const searchQuery = (query ?? "").trim();
    if (searchQuery) {
      params = params.set("query", searchQuery);
    }

    return this.http.get<ListResponse>(`${this.baseUrl}/users`, { params });
  }

  getById(id: string) {
    return this.http.get<ItemResponse>(`${this.baseUrl}/users/${id}`);
  }

  getAuditLogs(id: string) {
    return this.http.get<AuditLogListResponse>(`${this.baseUrl}/users/${id}/audit-logs`);
  }

  create(payload: CreateUserPayload) {
    return this.http.post<ItemResponse>(`${this.baseUrl}/users`, payload);
  }

  update(id: string, payload: UpdateUserPayload) {
    return this.http.put<ItemResponse>(`${this.baseUrl}/users/${id}`, payload);
  }

  remove(id: string) {
    return this.http.delete<{ ok: boolean }>(`${this.baseUrl}/users/${id}`);
  }

  getListAccessAuditLogs() {
    return this.http.get<AuditLogListResponse>(`${this.baseUrl}/users/audit-logs/list-access`);
  }
}


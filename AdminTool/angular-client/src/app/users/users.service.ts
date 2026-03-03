import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { CreateUserPayload, UpdateUserPayload, UserAuditLogItem, UserItem } from "../features/users/domain/user.models";

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
  private readonly baseUrl = "/api";

  constructor(private readonly http: HttpClient) {}

  list() {
    return this.http.get<ListResponse>(`${this.baseUrl}/users`);
  }

  getById(id: string) {
    return this.http.get<ItemResponse>(`${this.baseUrl}/users/${id}`);
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

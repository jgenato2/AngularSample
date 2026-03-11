import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";

export interface AuditLogListApiItem {
  id: string;
  entityId: string;
  action: string;
  field: string;
  oldValue: string | null;
  newValue: string | null;
  performedBy: string;
  occurredAtUtc: string;
}

export interface AuditLogListPaginationApi {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface AuditLogListApiResponse {
  items: AuditLogListApiItem[];
  pagination: AuditLogListPaginationApi;
}

@Injectable({ providedIn: "root" })
export class AuditLogApiService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = "/api/audit-logs";


  constructor() {}

  getListAccessAuditLogs(page: number, pageSize: number): Observable<AuditLogListApiResponse> {
    const params = new HttpParams()
      .set("page", String(page))
      .set("pageSize", String(pageSize));

    return this.http.get<AuditLogListApiResponse>(`${this.baseUrl}/list-access`, { params });
  }
}

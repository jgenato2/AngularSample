import { Injectable, inject } from "@angular/core";
import { map, Observable } from "rxjs";
import { AuditLogApiService } from "../infrastructure/audit-log-api.service";

export interface AuditLogListItem {
  id: string;
  entityId: string;
  occurredAtUtc: string;
  performedBy: string;
  action: string;
  field: string;
  oldValue: string | null;
  newValue: string | null;
}

export interface AuditLogPage {
  items: AuditLogListItem[];
  pagination: {
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
  };
}

@Injectable({ providedIn: "root" })
export class AuditLogFacade {
  private readonly auditLogApiService = inject(AuditLogApiService);


  constructor() {}

  getAllListAccessAuditLogs(page: number, pageSize: number): Observable<AuditLogPage> {
    return this.auditLogApiService.getListAccessAuditLogs(page, pageSize).pipe(
      map((response) => ({
        items: (response.items ?? []).map((item) => this.toAuditLogListItem(item)),
        pagination: {
          page: response.pagination?.page ?? page,
          pageSize: response.pagination?.pageSize ?? pageSize,
          totalItems: response.pagination?.totalItems ?? 0,
          totalPages: response.pagination?.totalPages ?? 1,
        },
      })),
    );
  }

  private toAuditLogListItem(item: {
    id: string;
    entityId: string;
    occurredAtUtc: string;
    performedBy: string;
    action: string;
    field: string;
    oldValue: string | null;
    newValue: string | null;
  }): AuditLogListItem {
    return {
      id: item.id,
      entityId: item.entityId,
      occurredAtUtc: item.occurredAtUtc,
      performedBy: item.performedBy,
      action: item.action,
      field: item.field,
      oldValue: item.oldValue,
      newValue: item.newValue,
    };
  }
}

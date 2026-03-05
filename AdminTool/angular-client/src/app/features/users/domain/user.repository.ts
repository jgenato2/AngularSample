import { InjectionToken } from "@angular/core";
import { Observable } from "rxjs";
import { CreateUserPayload, UpdateUserPayload, UserAuditLogItem, UserItem } from "./user.models";

export interface UserRepository {
  list(sort?: Array<{ field: string; direction: "asc" | "desc" }>, query?: string): Observable<UserItem[]>;
  getById(id: string): Observable<UserItem>;
  getAuditLogs(id: string): Observable<UserAuditLogItem[]>;
  create(payload: CreateUserPayload): Observable<UserItem>;
  update(id: string, payload: UpdateUserPayload): Observable<UserItem>;
  remove(id: string): Observable<boolean>;
  getListAccessAuditLogs(): Observable<UserAuditLogItem[]>;
}

export const USER_REPOSITORY = new InjectionToken<UserRepository>("USER_REPOSITORY");

import { Injectable, inject } from "@angular/core";
import { map } from "rxjs";
import { UsersService } from "../../../pages/users/users.service";
import { CreateUserPayload, UpdateUserPayload, UserAuditLogItem, UserItem } from "../domain/user.models";
import { UserRepository } from "../domain/user.repository";

@Injectable({ providedIn: "root" })
export class UserHttpRepository implements UserRepository {
  private readonly usersService = inject(UsersService);


  constructor() {}

  list(sort?: Array<{ field: string; direction: "asc" | "desc" }>, query?: string) {
    return this.usersService.list(sort, query).pipe(map((response) => response.items ?? []));
  }

  getById(id: string) {
    return this.usersService.getById(id).pipe(map((response) => response.item as UserItem));
  }

  getAuditLogs(id: string) {
    return this.usersService.getAuditLogs(id).pipe(map((response) => (response.items ?? []) as UserAuditLogItem[]));
  }

  create(payload: CreateUserPayload) {
    return this.usersService.create(payload).pipe(map((response) => response.item as UserItem));
  }

  update(id: string, payload: UpdateUserPayload) {
    return this.usersService.update(id, payload).pipe(map((response) => response.item as UserItem));
  }

  remove(id: string) {
    return this.usersService.remove(id).pipe(map((response) => !!response.ok));
  }

  getListAccessAuditLogs() {
    return this.usersService.getListAccessAuditLogs().pipe(map((response) => (response.items ?? []) as UserAuditLogItem[]));
  }
}

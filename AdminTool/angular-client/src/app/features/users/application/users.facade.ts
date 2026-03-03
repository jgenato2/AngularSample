import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { UsersService } from "../../../users/users.service";
import { CreateUserPayload, UpdateUserPayload, UserAuditLogItem, UserItem } from "../domain/user.models";

@Injectable({ providedIn: "root" })
export class UsersFacade {
  constructor(private readonly usersService: UsersService) {}

  list() {
    return this.usersService.list().pipe(map((response) => response.items ?? []));
  }

  getById(id: string) {
    return this.usersService.getById(id).pipe(map((response) => response.item as UserItem));
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

import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { UsersService } from "../../../../users/users.service";
import { UserAuditLogItem } from "../../domain/user.models";

@Injectable({ providedIn: "root" })
export class GetUsersListAccessAuditLogsQuery {
  constructor(private readonly usersService: UsersService) {}

  execute() {
    return this.usersService.getListAccessAuditLogs().pipe(map((response) => (response.items ?? []) as UserAuditLogItem[]));
  }
}

import { Injectable } from "@angular/core";
import { CreateUserCommand } from "./commands/create-user.command";
import { DeleteUserCommand } from "./commands/delete-user.command";
import { UpdateUserCommand } from "./commands/update-user.command";
import { GetUserAuditLogsQuery } from "./queries/get-user-audit-logs.query";
import { GetUserByIdQuery } from "./queries/get-user-by-id.query";
import { GetUsersListAccessAuditLogsQuery } from "./queries/get-users-list-access-audit-logs.query";
import { ListUsersQuery } from "./queries/list-users.query";
import { CreateUserPayload, UpdateUserPayload } from "../domain/user.models";

@Injectable({ providedIn: "root" })
export class UsersFacade {
  constructor(
    private readonly listUsersQuery: ListUsersQuery,
    private readonly getUserByIdQuery: GetUserByIdQuery,
    private readonly getUserAuditLogsQuery: GetUserAuditLogsQuery,
    private readonly createUserCommand: CreateUserCommand,
    private readonly updateUserCommand: UpdateUserCommand,
    private readonly deleteUserCommand: DeleteUserCommand,
    private readonly getUsersListAccessAuditLogsQuery: GetUsersListAccessAuditLogsQuery
  ) {}

  list() {
    return this.listUsersQuery.execute();
  }

  getById(id: string) {
    return this.getUserByIdQuery.execute(id);
  }

  getAuditLogs(id: string) {
    return this.getUserAuditLogsQuery.execute(id);
  }

  create(payload: CreateUserPayload) {
    return this.createUserCommand.execute(payload);
  }

  update(id: string, payload: UpdateUserPayload) {
    return this.updateUserCommand.execute(id, payload);
  }

  remove(id: string) {
    return this.deleteUserCommand.execute(id);
  }

  getListAccessAuditLogs() {
    return this.getUsersListAccessAuditLogsQuery.execute();
  }
}

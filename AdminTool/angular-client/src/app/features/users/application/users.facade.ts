import { Injectable, inject } from "@angular/core";
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
  private readonly listUsersQuery = inject(ListUsersQuery);
  private readonly getUserByIdQuery = inject(GetUserByIdQuery);
  private readonly getUserAuditLogsQuery = inject(GetUserAuditLogsQuery);
  private readonly createUserCommand = inject(CreateUserCommand);
  private readonly updateUserCommand = inject(UpdateUserCommand);
  private readonly deleteUserCommand = inject(DeleteUserCommand);
  private readonly getUsersListAccessAuditLogsQuery = inject(GetUsersListAccessAuditLogsQuery);


  constructor() {}

  list(sort?: Array<{ field: string; direction: "asc" | "desc" }>, query?: string) {
    return this.listUsersQuery.execute(sort, query);
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

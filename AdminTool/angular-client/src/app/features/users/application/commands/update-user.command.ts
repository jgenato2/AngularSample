import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { UsersService } from "../../../../users/users.service";
import { UpdateUserPayload, UserItem } from "../../domain/user.models";

@Injectable({ providedIn: "root" })
export class UpdateUserCommand {
  constructor(private readonly usersService: UsersService) {}

  execute(id: string, payload: UpdateUserPayload) {
    return this.usersService.update(id, payload).pipe(map((response) => response.item as UserItem));
  }
}

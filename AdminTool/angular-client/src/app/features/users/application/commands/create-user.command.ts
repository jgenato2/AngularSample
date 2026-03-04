import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { UsersService } from "../../../../users/users.service";
import { CreateUserPayload, UserItem } from "../../domain/user.models";

@Injectable({ providedIn: "root" })
export class CreateUserCommand {
  constructor(private readonly usersService: UsersService) {}

  execute(payload: CreateUserPayload) {
    return this.usersService.create(payload).pipe(map((response) => response.item as UserItem));
  }
}

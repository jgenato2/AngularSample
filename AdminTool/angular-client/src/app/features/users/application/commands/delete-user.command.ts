import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { UsersService } from "../../../../users/users.service";

@Injectable({ providedIn: "root" })
export class DeleteUserCommand {
  constructor(private readonly usersService: UsersService) {}

  execute(id: string) {
    return this.usersService.remove(id).pipe(map((response) => !!response.ok));
  }
}

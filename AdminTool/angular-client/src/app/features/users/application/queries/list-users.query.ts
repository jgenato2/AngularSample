import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { UsersService } from "../../../../users/users.service";

@Injectable({ providedIn: "root" })
export class ListUsersQuery {
  constructor(private readonly usersService: UsersService) {}

  execute() {
    return this.usersService.list().pipe(map((response) => response.items ?? []));
  }
}

import { Injectable } from "@angular/core";
import { map } from "rxjs";
import { UsersService } from "../../../../users/users.service";
import { UserItem } from "../../domain/user.models";

@Injectable({ providedIn: "root" })
export class GetUserByIdQuery {
  constructor(private readonly usersService: UsersService) {}

  execute(id: string) {
    return this.usersService.getById(id).pipe(map((response) => response.item as UserItem));
  }
}

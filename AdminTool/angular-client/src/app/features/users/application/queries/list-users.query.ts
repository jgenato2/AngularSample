import { Inject, Injectable } from "@angular/core";
import { USER_REPOSITORY, UserRepository } from "../../domain/user.repository";

@Injectable({ providedIn: "root" })
export class ListUsersQuery {
  constructor(@Inject(USER_REPOSITORY) private readonly userRepository: UserRepository) {}

  execute() {
    return this.userRepository.list();
  }
}

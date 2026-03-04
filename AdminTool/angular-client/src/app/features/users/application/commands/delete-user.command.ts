import { Inject, Injectable } from "@angular/core";
import { USER_REPOSITORY, UserRepository } from "../../domain/user.repository";

@Injectable({ providedIn: "root" })
export class DeleteUserCommand {
  constructor(@Inject(USER_REPOSITORY) private readonly userRepository: UserRepository) {}

  execute(id: string) {
    return this.userRepository.remove(id);
  }
}

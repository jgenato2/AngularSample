import { Inject, Injectable } from "@angular/core";
import { UpdateUserPayload } from "../../domain/user.models";
import { USER_REPOSITORY, UserRepository } from "../../domain/user.repository";

@Injectable({ providedIn: "root" })
export class UpdateUserCommand {
  constructor(@Inject(USER_REPOSITORY) private readonly userRepository: UserRepository) {}

  execute(id: string, payload: UpdateUserPayload) {
    return this.userRepository.update(id, payload);
  }
}

import { Inject, Injectable } from "@angular/core";
import { CreateUserPayload } from "../../domain/user.models";
import { USER_REPOSITORY, UserRepository } from "../../domain/user.repository";

@Injectable({ providedIn: "root" })
export class CreateUserCommand {
  constructor(@Inject(USER_REPOSITORY) private readonly userRepository: UserRepository) {}

  execute(payload: CreateUserPayload) {
    return this.userRepository.create(payload);
  }
}

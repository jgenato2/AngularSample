import { ChangeDetectorRef, Component, OnInit } from "@angular/core";
import { CommonModule, DatePipe } from "@angular/common";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { AuthService } from "../../core/auth.service";
import { UsersService, UserItem } from "../users.service";
import { HttpErrorResponse } from "@angular/common/http";
import { finalize } from "rxjs";
import { Modal } from "bootstrap";
import {
  UserFormDialogComponent,
  UserDialogData,
} from "../user-form-dialog/user-form-dialog.component";

@Component({
  selector: "app-user-detail",
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    UserFormDialogComponent,
  ],
  providers: [DatePipe],
  templateUrl: "./user-detail.component.html",
  styleUrl: "./user-detail.component.scss",
})
export class UserDetailComponent implements OnInit {
  user: UserItem | null = null;
  loading = false;
  alertMessage: string | null = null;
  alertType: "danger" | "success" = "danger";
  editModalId = "edit-user-modal";
  editDialogData: UserDialogData = { mode: "edit", isAdmin: false };

  constructor(
    private readonly route: ActivatedRoute,
    private readonly usersService: UsersService,
    private readonly router: Router,
    public readonly auth: AuthService,
    public readonly datePipe: DatePipe,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.load();
  }

  get canEdit() {
    const current = this.auth.user();
    return !!this.user && !!current && (current.role === "admin" || current.id === this.user.id);
  }

  load() {
    const id = this.route.snapshot.paramMap.get("id");
    if (!id) {
      return;
    }

    this.loading = true;
    this.usersService
      .getById(id)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (response) => {
          this.user = response.item;
          this.cdr.markForCheck();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.auth.logout();
            this.router.navigateByUrl("/login");
            return;
          }
          this.setAlert("User not found.");
          this.router.navigateByUrl("/users");
        },
      });
  }

  openEdit() {
    if (!this.user) {
      return;
    }
    this.editDialogData = {
      mode: "edit",
      user: this.user,
      isAdmin: this.auth.isAdmin(),
    };
  }

  handleEditSubmit(result: {
    name: string;
    email: string;
    role?: "admin" | "user";
    password?: string;
  }) {
    if (!this.user) {
      return;
    }

    this.usersService
      .update(this.user.id, {
        name: result.name,
        email: result.email,
        role: this.auth.isAdmin() ? result.role : undefined,
        password: result.password || undefined,
      })
      .subscribe({
        next: (response) => {
          queueMicrotask(() => {
            this.user = response.item;
            this.cdr.detectChanges();
          });
          this.setAlert("User updated.", "success");
          this.hideModal(this.editModalId);
        },
        error: () => {
          this.setAlert("Failed to update user.");
        },
      });
  }

  remove() {
    if (!this.user) {
      return;
    }
    if (!confirm(`Delete ${this.user.name}?`)) {
      return;
    }

    this.usersService.remove(this.user.id).subscribe({
      next: () => {
        this.setAlert("User deleted.", "success");
        this.router.navigateByUrl("/users");
      },
      error: () => {
        this.setAlert("Failed to delete user.");
      },
    });
  }

  private setAlert(message: string, type: "danger" | "success" = "danger") {
    this.alertMessage = message;
    this.alertType = type;
  }

  private hideModal(id: string) {
    const element = document.getElementById(id);
    if (!element) {
      this.cleanupModalArtifacts();
      return;
    }
    const modal = Modal.getOrCreateInstance(element);
    modal?.hide();
    window.setTimeout(() => this.cleanupModalArtifacts(), 250);
  }

  private cleanupModalArtifacts() {
    document.body.classList.remove("modal-open");
    document.body.style.removeProperty("padding-right");

    const backdrops = document.querySelectorAll(".modal-backdrop");
    backdrops.forEach((backdrop) => backdrop.remove());
  }
}

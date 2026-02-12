import { ChangeDetectorRef, Component, OnInit } from "@angular/core";
import { CommonModule, DatePipe } from "@angular/common";
import { ActivatedRoute, Router, RouterLink } from "@angular/router";
import { MatButtonModule } from "@angular/material/button";
import { MatCardModule } from "@angular/material/card";
import { MatDialog, MatDialogModule } from "@angular/material/dialog";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { AuthService } from "../../core/auth.service";
import { UsersService, UserItem } from "../users.service";
import { HttpErrorResponse } from "@angular/common/http";
import { finalize } from "rxjs";
import { UserFormDialogComponent } from "../user-form-dialog/user-form-dialog.component";

@Component({
  selector: "app-user-detail",
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  providers: [DatePipe],
  templateUrl: "./user-detail.component.html",
  styleUrl: "./user-detail.component.scss",
})
export class UserDetailComponent implements OnInit {
  user: UserItem | null = null;
  loading = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly usersService: UsersService,
    private readonly router: Router,
    private readonly dialog: MatDialog,
    private readonly snackBar: MatSnackBar,
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
          this.snackBar.open("User not found.", "Dismiss", { duration: 3000 });
          this.router.navigateByUrl("/users");
        },
      });
  }

  openEdit() {
    if (!this.user) {
      return;
    }

    const dialogRef = this.dialog.open(UserFormDialogComponent, {
      data: {
        mode: "edit",
        user: this.user,
        isAdmin: this.auth.isAdmin(),
      },
      width: "420px",
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.usersService
        .update(this.user!.id, {
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
            this.snackBar.open("User updated.", "Dismiss", { duration: 2500 });
          },
          error: () => {
            this.snackBar.open("Failed to update user.", "Dismiss", { duration: 3000 });
          },
        });
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
        this.snackBar.open("User deleted.", "Dismiss", { duration: 2500 });
        this.router.navigateByUrl("/users");
      },
      error: () => {
        this.snackBar.open("Failed to delete user.", "Dismiss", { duration: 3000 });
      },
    });
  }
}

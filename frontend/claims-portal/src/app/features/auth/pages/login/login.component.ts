import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal
} from '@angular/core';

import { HttpErrorResponse } from '@angular/common/http';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../../../core/auth/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent {

  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loginForm = this.formBuilder.nonNullable.group({

    email: [
      '',
      [
        Validators.required,
        Validators.email
      ]
    ],

    password: [
      '',
      Validators.required
    ]

  });

  // Signals automatically notify Angular when their values change.
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');

  submit(): void {

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const credentials = this.loginForm.getRawValue();

    this.authService.login(credentials)
      .pipe(
        finalize(() => {
          this.isSubmitting.set(false);
        })
      )
      .subscribe({

        next: () => {

          const destination =
            this.authService.getHomeRoute();

          void this.router.navigateByUrl(destination);
        },

        error: (error: unknown) => {

          this.errorMessage.set(
            this.getErrorMessage(error)
          );

        }

      });
  }

  private getErrorMessage(error: unknown): string {

    if (error instanceof HttpErrorResponse) {

      if (error.status === 401) {
        return 'Invalid email or password.';
      }

      if (error.status === 0) {
        return 'Unable to connect to the server. Please check that the API is running.';
      }

      if (error.status === 400) {
        return 'Please check your email and password.';
      }

      const problem = error.error as {
        title?: string;
        detail?: string;
      };

      return problem?.detail ||
        problem?.title ||
        'An unexpected error occurred. Please try again.';
    }

    return 'Unable to sign in. Please try again.';
  }
}

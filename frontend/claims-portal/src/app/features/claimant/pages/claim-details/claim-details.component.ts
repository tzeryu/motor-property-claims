import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';

import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { finalize } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

import { ClaimsApiService } from '../../../../core/claims/services/claims-api.service';
import {
  ClaimDetails,
  ClaimStatus
} from '../../../../core/claims/models/claim.models';

@Component({
  selector: 'app-claim-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    DatePipe,
    CurrencyPipe
  ],
  templateUrl: './claim-details.component.html',
  styleUrl: './claim-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClaimDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly claimsApi = inject(ClaimsApiService);

  readonly claim = signal<ClaimDetails | null>(null);

  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);

  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  private claimId: string | null = null;

  readonly responseForm = this.fb.nonNullable.group({
    response: [
      '',
      [
        Validators.required,
        Validators.maxLength(4000)
      ]
    ]
  });

  ngOnInit(): void {
    this.claimId = this.route.snapshot.paramMap.get('id');

    if (!this.claimId) {
      this.isLoading.set(false);
      this.errorMessage.set('Invalid claim ID.');
      return;
    }

    this.loadClaim();
  }

  private loadClaim(): void {
    if (!this.claimId) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.claimsApi.getClaimById(this.claimId)
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (result) => {
          this.claim.set(result);
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to load claim details.'
            )
          );
        }
      });
  }

  submitResponse(): void {
    this.responseForm.markAllAsTouched();

    if (this.responseForm.invalid || !this.claimId) {
      return;
    }

    const response =
      this.responseForm.controls.response.value.trim();

    if (!response) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.claimsApi.provideAdditionalInformation(
      this.claimId,
      { response }
    )
      .pipe(
        finalize(() => this.isSubmitting.set(false))
      )
      .subscribe({
        next: () => {
          this.successMessage.set(
            'Your additional information has been submitted successfully.'
          );

          this.responseForm.reset({
            response: ''
          });

          // Refresh the claim to retrieve its latest status
          // and the recorded information response.
          this.loadClaim();
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to submit your response. Please try again.'
            )
          );
        }
      });
  }

  isAwaitingInformation(status: ClaimStatus): boolean {
    return status === 'awaitingInformation';
  }

  formatStatus(status: ClaimStatus): string {
    const labels: Record<ClaimStatus, string> = {
      draft: 'Draft',
      submitted: 'Submitted',
      inReview: 'In Review',
      awaitingInformation: 'Awaiting Information',
      pendingManagerApproval: 'Pending Manager Approval',
      approved: 'Approved',
      rejected: 'Rejected',
      settled: 'Settled'
    };

    return labels[status];
  }

  private getErrorMessage(
    error: unknown,
    fallback: string
  ): string {
    if (error instanceof HttpErrorResponse) {
      const body = error.error as {
        title?: string;
        detail?: string;
        message?: string;
      } | null;

      return body?.detail
        ?? body?.title
        ?? body?.message
        ?? fallback;
    }

    return fallback;
  }
}

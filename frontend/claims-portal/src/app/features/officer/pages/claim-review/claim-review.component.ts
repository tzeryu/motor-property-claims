import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { HttpErrorResponse } from '@angular/common/http';
import { finalize, Observable } from 'rxjs';

import { ClaimsApiService } from '../../../../core/claims/services/claims-api.service';

import {
  ClaimDetails,
  ClaimStatus
} from '../../../../core/claims/models/claim.models';

type ReviewAction =
  | 'startReview'
  | 'requestInformation'
  | 'approve'
  | 'reject';

@Component({
  selector: 'app-claim-review',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule
  ],
  templateUrl: './claim-review.component.html',
  styleUrl: './claim-review.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClaimReviewComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly claimsApi = inject(ClaimsApiService);

  readonly claim = signal<ClaimDetails | null>(null);

  readonly isLoading = signal(true);
  readonly activeAction = signal<ReviewAction | null>(null);

  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  private claimId: string | null = null;

  readonly informationForm = this.fb.nonNullable.group({
    message: [
      '',
      [
        Validators.required,
        Validators.maxLength(2000)
      ]
    ]
  });

  readonly approvalForm = this.fb.nonNullable.group({
    settlementAmount: [
      0,
      [
        Validators.required,
        Validators.min(0.01)
      ]
    ]
  });

  readonly rejectionForm = this.fb.nonNullable.group({
    reason: ['', Validators.required]
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

  loadClaim(): void {
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
              'Unable to retrieve claim details.'
            )
          );
        }
      });
  }

  startReview(): void {
    if (!this.claimId || this.activeAction()) {
      return;
    }

    this.executeAction(
      'startReview',
      this.claimsApi.startReview(this.claimId),
      'Claim review has started successfully.'
    );
  }

  requestInformation(): void {
    this.informationForm.markAllAsTouched();

    if (
      this.informationForm.invalid ||
      !this.claimId ||
      this.activeAction()
    ) {
      return;
    }

    const message =
      this.informationForm.controls.message.value.trim();

    if (!message) {
      return;
    }

    this.executeAction(
      'requestInformation',
      this.claimsApi.requestAdditionalInformation(
        this.claimId,
        { message }
      ),
      'Additional information has been requested from the claimant.'
    );

    this.informationForm.reset({
      message: ''
    });
  }

  approveClaim(): void {
    this.approvalForm.markAllAsTouched();

    if (
      this.approvalForm.invalid ||
      !this.claimId ||
      this.activeAction()
    ) {
      return;
    }

    const settlementAmount =
      this.approvalForm.controls.settlementAmount.value;

    this.activeAction.set('approve');
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.claimsApi.approveClaim(
      this.claimId,
      { settlementAmount }
    )
      .pipe(
        finalize(() => this.activeAction.set(null))
      )
      .subscribe({
        next: (result) => {
          this.successMessage.set(
            result.managerApprovalRequired
              ? 'The proposed settlement has been submitted for manager approval.'
              : 'The claim has been approved successfully.'
          );

          this.approvalForm.reset({
            settlementAmount: 0
          });

          this.loadClaim();
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to approve the claim.'
            )
          );
        }
      });
  }

  rejectClaim(): void {
    this.rejectionForm.markAllAsTouched();

    if (
      this.rejectionForm.invalid ||
      !this.claimId ||
      this.activeAction()
    ) {
      return;
    }

    const reason =
      this.rejectionForm.controls.reason.value.trim();

    if (!reason) {
      return;
    }

    this.executeAction(
      'reject',
      this.claimsApi.rejectClaim(
        this.claimId,
        { reason }
      ),
      'The claim has been rejected successfully.'
    );

    this.rejectionForm.reset({
      reason: ''
    });
  }

  private executeAction(
    action: ReviewAction,
    request: Observable<void>,
    successMessage: string
  ): void {
    this.activeAction.set(action);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    request
      .pipe(
        finalize(() => this.activeAction.set(null))
      )
      .subscribe({
        next: () => {
          this.successMessage.set(successMessage);
          this.loadClaim();
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'The requested action could not be completed.'
            )
          );
        }
      });
  }

  isStatus(status: ClaimStatus): boolean {
    return this.claim()?.status === status;
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

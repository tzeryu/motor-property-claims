import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';

import { ClaimsApiService } from '../../../../core/claims/services/claims-api.service';

import {
  ClaimDetails,
  ClaimListItem,
  ClaimStatus,
  PaymentOutcome,
  PagedClaimsResult
} from '../../../../core/claims/models/claim.models';

type SettlementQueueStatus = 'approved' | 'settled';
type SettlementAction = 'settlement' | 'payment' | null;

@Component({
  selector: 'app-settlement-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './settlement-management.component.html',
  styleUrl: './settlement-management.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettlementManagementComponent implements OnInit {
  private readonly claimsApi = inject(ClaimsApiService);
  private readonly fb = inject(FormBuilder);

  readonly queueStatus = signal<SettlementQueueStatus>('approved');
  readonly pageNumber = signal(1);
  readonly pageSize = 10;

  readonly queuePage = signal<PagedClaimsResult | null>(null);
  readonly selectedClaim = signal<ClaimDetails | null>(null);

  readonly isQueueLoading = signal(false);
  readonly isDetailLoading = signal(false);
  readonly activeAction = signal<SettlementAction>(null);

  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly showSettlementConfirmation = signal(false);
  readonly showPaymentConfirmation = signal(false);
  readonly paymentReferenceRequired = signal(false);

  readonly paymentForm = this.fb.nonNullable.group({
    status: ['paid' as PaymentOutcome, Validators.required],
    paymentReference: [
      '',
      [
        Validators.maxLength(100)
      ]
    ]
  });

  ngOnInit(): void {
    this.loadQueue();
  }

  loadQueue(): void {
    this.isQueueLoading.set(true);
    this.errorMessage.set(null);

    this.claimsApi.getStaffClaims(
      this.pageNumber(),
      this.pageSize,
      this.queueStatus()
    )
      .pipe(
        finalize(() => this.isQueueLoading.set(false))
      )
      .subscribe({
        next: (result) => {
          this.queuePage.set(result);
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to load settlement records.'
            )
          );
        }
      });
  }

  changeQueueStatus(status: SettlementQueueStatus): void {
    if (this.queueStatus() === status) {
      return;
    }

    this.queueStatus.set(status);
    this.pageNumber.set(1);
    this.clearSelection();
    this.loadQueue();
  }

  selectClaim(item: ClaimListItem): void {
    this.loadClaimDetails(item.id);
  }

  private loadClaimDetails(id: string): void {
    this.isDetailLoading.set(true);
    this.errorMessage.set(null);

    this.showSettlementConfirmation.set(false);
    this.showPaymentConfirmation.set(false);
    this.paymentReferenceRequired.set(false);

    this.claimsApi.getClaimById(id)
      .pipe(
        finalize(() => this.isDetailLoading.set(false))
      )
      .subscribe({
        next: (claim) => {
          this.selectedClaim.set(claim);

          this.paymentForm.reset({
            status: 'paid',
            paymentReference: claim.paymentReference ?? ''
          });
        },
        error: (error: unknown) => {
          this.selectedClaim.set(null);

          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to retrieve claim details.'
            )
          );
        }
      });
  }

  clearSelection(): void {
    this.selectedClaim.set(null);
    this.showSettlementConfirmation.set(false);
    this.showPaymentConfirmation.set(false);
    this.paymentReferenceRequired.set(false);

    this.paymentForm.reset({
      status: 'paid',
      paymentReference: ''
    });
  }

  canRecordSettlement(claim: ClaimDetails): boolean {
    return claim.status === 'approved'
      && claim.settlementDateUtc === null;
  }

  canUpdatePayment(claim: ClaimDetails): boolean {
    return claim.status === 'approved'
      && claim.settlementDateUtc !== null
      && claim.paymentStatus !== 'paid';
  }

  requestSettlementConfirmation(): void {
    const claim = this.selectedClaim();

    if (
      !claim ||
      !this.canRecordSettlement(claim) ||
      this.activeAction()
    ) {
      return;
    }

    this.showSettlementConfirmation.set(true);
    this.errorMessage.set(null);
  }

  cancelSettlementConfirmation(): void {
    this.showSettlementConfirmation.set(false);
  }

  confirmRecordSettlement(): void {
    const claim = this.selectedClaim();

    if (
      !claim ||
      !this.canRecordSettlement(claim) ||
      this.activeAction()
    ) {
      return;
    }

    this.activeAction.set('settlement');
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.claimsApi.recordSettlement(claim.id)
      .pipe(
        finalize(() => this.activeAction.set(null))
      )
      .subscribe({
        next: () => {
          this.showSettlementConfirmation.set(false);

          this.successMessage.set(
            'Settlement has been recorded. Payment is now pending.'
          );

          this.loadClaimDetails(claim.id);
          this.loadQueue();
        },
        error: (error: unknown) => {
          this.showSettlementConfirmation.set(false);

          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to record settlement.'
            )
          );

          this.loadClaimDetails(claim.id);
        }
      });
  }

  onPaymentStatusChange(value: string): void {
    if (value !== 'paid' && value !== 'failed') {
      return;
    }

    this.paymentForm.controls.status.setValue(value);
    this.paymentReferenceRequired.set(false);
    this.showPaymentConfirmation.set(false);
    this.errorMessage.set(null);
  }

  requestPaymentConfirmation(): void {
    const claim = this.selectedClaim();

    if (
      !claim ||
      !this.canUpdatePayment(claim) ||
      this.activeAction()
    ) {
      return;
    }

    this.paymentForm.markAllAsTouched();

    const status = this.paymentForm.controls.status.value;
    const reference =
      this.paymentForm.controls.paymentReference.value.trim();

    if (
      status === 'paid' &&
      !reference
    ) {
      this.paymentReferenceRequired.set(true);
      return;
    }

    if (this.paymentForm.invalid) {
      return;
    }

    this.paymentReferenceRequired.set(false);
    this.showPaymentConfirmation.set(true);
    this.errorMessage.set(null);
  }

  cancelPaymentConfirmation(): void {
    this.showPaymentConfirmation.set(false);
  }

  confirmPaymentOutcome(): void {
    const claim = this.selectedClaim();

    if (
      !claim ||
      !this.canUpdatePayment(claim) ||
      !this.showPaymentConfirmation() ||
      this.activeAction()
    ) {
      return;
    }

    const status = this.paymentForm.controls.status.value;
    const reference =
      this.paymentForm.controls.paymentReference.value.trim();

    if (status === 'paid' && !reference) {
      this.paymentReferenceRequired.set(true);
      this.showPaymentConfirmation.set(false);
      return;
    }

    this.activeAction.set('payment');
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.claimsApi.updatePaymentStatus(
      claim.id,
      {
        status,
        paymentReference: reference || null
      }
    )
      .pipe(
        finalize(() => this.activeAction.set(null))
      )
      .subscribe({
        next: () => {
          this.showPaymentConfirmation.set(false);

          if (status === 'paid') {
            this.successMessage.set(
              'Payment recorded successfully. The claim is now settled.'
            );

            // A successful payment moves the claim from Approved
            // to Settled, so move the queue to the settled view.
            this.queueStatus.set('settled');
            this.pageNumber.set(1);
          } else {
            this.successMessage.set(
              'Payment has been marked as failed. The claim remains approved and can be retried.'
            );
          }

          this.loadClaimDetails(claim.id);
          this.loadQueue();
        },
        error: (error: unknown) => {
          this.showPaymentConfirmation.set(false);

          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to update payment status.'
            )
          );

          this.loadClaimDetails(claim.id);
        }
      });
  }

  previousPage(): void {
    if (this.pageNumber() <= 1) {
      return;
    }

    this.pageNumber.update(page => page - 1);
    this.clearSelection();
    this.loadQueue();
  }

  nextPage(): void {
    const page = this.queuePage();

    if (
      !page ||
      this.pageNumber() >= page.totalPages
    ) {
      return;
    }

    this.pageNumber.update(page => page + 1);
    this.clearSelection();
    this.loadQueue();
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

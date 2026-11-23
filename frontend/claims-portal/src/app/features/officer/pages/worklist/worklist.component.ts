import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';

import { ClaimsApiService } from '../../../../core/claims/services/claims-api.service';

import {
  ClaimStatus,
  PagedClaimsResult
} from '../../../../core/claims/models/claim.models';

type AssignmentFilter = 'all' | 'assignedToMe' | 'unassigned';
type StatusFilter = 'all' | ClaimStatus;

@Component({
  selector: 'app-officer-worklist',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './worklist.component.html',
  styleUrl: './worklist.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorklistComponent implements OnInit {
  private readonly claimsApi = inject(ClaimsApiService);
  private readonly router = inject(Router);

  readonly result = signal<PagedClaimsResult | null>(null);

  readonly isLoading = signal(false);
  readonly startingClaimId = signal<string | null>(null);

  readonly errorMessage = signal<string | null>(null);

  readonly pageNumber = signal(1);
  readonly pageSize = 10;

  readonly statusFilter = signal<StatusFilter>('all');
  readonly assignmentFilter = signal<AssignmentFilter>('all');

  readonly statusOptions: {
    value: ClaimStatus;
    label: string;
  }[] = [
      { value: 'submitted', label: 'Submitted' },
      { value: 'inReview', label: 'In Review' },
      { value: 'awaitingInformation', label: 'Awaiting Information' },
      { value: 'pendingManagerApproval', label: 'Pending Manager Approval' },
      { value: 'approved', label: 'Approved' },
      { value: 'rejected', label: 'Rejected' },
      { value: 'settled', label: 'Settled' }
    ];

  ngOnInit(): void {
    this.loadClaims();
  }

  loadClaims(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const assignment = this.assignmentFilter();
    const status = this.statusFilter();

    this.claimsApi.getStaffClaims(
      this.pageNumber(),
      this.pageSize,
      status === 'all' ? undefined : status,
      assignment === 'assignedToMe',
      assignment === 'unassigned'
    )
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (response) => {
          this.result.set(response);
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to load the claims worklist.'
            )
          );
        }
      });
  }

  onStatusChange(value: string): void {
    if (
      value !== 'all' &&
      !this.statusOptions.some(option => option.value === value)
    ) {
      return;
    }

    this.statusFilter.set(value as StatusFilter);
    this.pageNumber.set(1);
    this.loadClaims();
  }

  onAssignmentChange(value: string): void {
    if (
      value !== 'all' &&
      value !== 'assignedToMe' &&
      value !== 'unassigned'
    ) {
      return;
    }

    this.assignmentFilter.set(value);
    this.pageNumber.set(1);
    this.loadClaims();
  }

  startReview(claimId: string): void {
    this.startingClaimId.set(claimId);
    this.errorMessage.set(null);

    this.claimsApi.startReview(claimId)
      .pipe(
        finalize(() => this.startingClaimId.set(null))
      )
      .subscribe({
        next: () => {
          // The backend has assigned this claim to the
          // authenticated officer and moved it into review.
          this.router.navigate(['/officer/claims', claimId]);
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            error instanceof HttpErrorResponse && error.status === 409
              ? 'This claim may have been picked up by another officer. Refresh the worklist.'
              : this.getErrorMessage(
                error,
                'Unable to start the claim review.'
              )
          );

          this.loadClaims();
        }
      });
  }

  previousPage(): void {
    if (this.pageNumber() <= 1) {
      return;
    }

    this.pageNumber.update(page => page - 1);
    this.loadClaims();
  }

  nextPage(): void {
    const currentResult = this.result();

    if (
      !currentResult ||
      this.pageNumber() >= currentResult.totalPages
    ) {
      return;
    }

    this.pageNumber.update(page => page + 1);
    this.loadClaims();
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

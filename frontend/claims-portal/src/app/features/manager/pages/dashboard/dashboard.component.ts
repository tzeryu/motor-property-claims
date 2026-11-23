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
import { finalize, forkJoin } from 'rxjs';

import { ClaimsApiService } from '../../../../core/claims/services/claims-api.service';

import {
  ClaimDetails,
  ClaimListItem,
  ManagementOverview,
  PagedClaimsResult
} from '../../../../core/claims/models/claim.models';

type ManagerDecision = 'approve' | 'reject' | null;

@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent implements OnInit {
  private readonly claimsApi = inject(ClaimsApiService);
  private readonly fb = inject(FormBuilder);

  readonly overview = signal<ManagementOverview | null>(null);
  readonly queuePage = signal<PagedClaimsResult | null>(null);
  readonly selectedClaim = signal<ClaimDetails | null>(null);

  readonly isLoading = signal(false);
  readonly isQueueLoading = signal(false);
  readonly isDetailLoading = signal(false);

  readonly activeAction = signal<ManagerDecision>(null);
  readonly pendingDecision = signal<ManagerDecision>(null);

  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly pageNumber = signal(1);
  readonly pageSize = 10;

  readonly rejectionForm = this.fb.nonNullable.group({
    reason: ['', Validators.required]
  });

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      overview: this.claimsApi.getManagementOverview(),
      queue: this.claimsApi.getStaffClaims(
        this.pageNumber(),
        this.pageSize,
        'pendingManagerApproval'
      )
    })
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: ({ overview, queue }) => {
          this.overview.set(overview);
          this.queuePage.set(queue);
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to load the management dashboard.'
            )
          );
        }
      });
  }

  loadQueue(): void {
    this.isQueueLoading.set(true);
    this.errorMessage.set(null);

    this.claimsApi.getStaffClaims(
      this.pageNumber(),
      this.pageSize,
      'pendingManagerApproval'
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
              'Unable to load the approval queue.'
            )
          );
        }
      });
  }

  selectClaim(item: ClaimListItem): void {
    this.selectedClaim.set(null);
    this.isDetailLoading.set(true);
    this.errorMessage.set(null);
    this.pendingDecision.set(null);
    this.rejectionForm.reset({ reason: '' });

    this.claimsApi.getClaimById(item.id)
      .pipe(
        finalize(() => this.isDetailLoading.set(false))
      )
      .subscribe({
        next: (claim) => {
          this.selectedClaim.set(claim);
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to retrieve the selected claim.'
            )
          );
        }
      });
  }

  clearSelection(): void {
    this.selectedClaim.set(null);
    this.pendingDecision.set(null);
    this.rejectionForm.reset({ reason: '' });
  }

  prepareApproval(): void {
    if (
      !this.selectedClaim() ||
      this.selectedClaim()?.status !== 'pendingManagerApproval' ||
      this.activeAction()
    ) {
      return;
    }

    this.pendingDecision.set('approve');
    this.errorMessage.set(null);
  }

  prepareRejection(): void {
    this.rejectionForm.markAllAsTouched();

    const reason = this.rejectionForm.controls.reason.value.trim();

    if (
      this.rejectionForm.invalid ||
      !reason ||
      !this.selectedClaim() ||
      this.selectedClaim()?.status !== 'pendingManagerApproval' ||
      this.activeAction()
    ) {
      return;
    }

    this.pendingDecision.set('reject');
    this.errorMessage.set(null);
  }

  cancelDecision(): void {
    this.pendingDecision.set(null);
  }

  confirmDecision(): void {
    const claim = this.selectedClaim();
    const decision = this.pendingDecision();

    if (
      !claim ||
      claim.status !== 'pendingManagerApproval' ||
      !decision ||
      this.activeAction()
    ) {
      return;
    }

    if (decision === 'approve') {
      this.confirmApproval(claim.id);
      return;
    }

    this.confirmRejection(claim.id);
  }

  private confirmApproval(claimId: string): void {
    this.activeAction.set('approve');
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.claimsApi.managerApproveClaim(claimId)
      .pipe(
        finalize(() => this.activeAction.set(null))
      )
      .subscribe({
        next: () => {
          this.successMessage.set(
            'The claim has been approved successfully.'
          );

          this.clearSelection();
          this.loadDashboard();
        },
        error: (error: unknown) => {
          this.pendingDecision.set(null);
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to approve this claim.'
            )
          );
        }
      });
  }

  private confirmRejection(claimId: string): void {
    const reason = this.rejectionForm.controls.reason.value.trim();

    if (!reason) {
      return;
    }

    this.activeAction.set('reject');
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.claimsApi.managerRejectClaim(
      claimId,
      { reason }
    )
      .pipe(
        finalize(() => this.activeAction.set(null))
      )
      .subscribe({
        next: () => {
          this.successMessage.set(
            'The claim has been rejected successfully.'
          );

          this.clearSelection();
          this.loadDashboard();
        },
        error: (error: unknown) => {
          this.pendingDecision.set(null);
          this.errorMessage.set(
            this.getErrorMessage(
              error,
              'Unable to reject this claim.'
            )
          );
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

    if (!page || this.pageNumber() >= page.totalPages) {
      return;
    }

    this.pageNumber.update(current => current + 1);
    this.clearSelection();
    this.loadQueue();
  }

  formatClaimType(value: string): string {
    return value.toLowerCase() === 'motor' ? 'Motor' : 'Property';
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

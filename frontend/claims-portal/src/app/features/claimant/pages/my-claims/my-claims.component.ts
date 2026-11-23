import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  inject,
  signal
} from '@angular/core';

import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { ClaimsApiService } from '../../../../core/claims/services/claims-api.service';

import {
  ClaimListItem,
  PagedClaimsResult
} from '../../../../core/claims/models/claim.models';


@Component({
  selector: 'app-my-claims',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink
  ],
  templateUrl: './my-claims.component.html',
  styleUrl: './my-claims.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyClaimsComponent implements OnInit {

  private readonly claimsApi = inject(ClaimsApiService);

  readonly claims = signal<ClaimListItem[]>([]);

  readonly pageNumber = signal(1);
  readonly pageSize = 10;

  readonly totalCount = signal(0);
  readonly totalPages = signal(1);

  readonly selectedStatus = signal('');

  readonly isLoading = signal(false);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.loadClaims();
  }

  loadClaims(): void {

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.claimsApi.getMyClaims(
      this.pageNumber(),
      this.pageSize,
      this.selectedStatus() || undefined
    )
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        })
      )
      .subscribe({

        next: (result: PagedClaimsResult) => {
          this.updateClaims(result);
        },

        error: () => {
          this.errorMessage.set(
            'Unable to retrieve your claims. Please try again.'
          );
        }

      });
  }

  onStatusChange(event: Event): void {

    const select = event.target as HTMLSelectElement;

    this.selectedStatus.set(select.value);
    this.pageNumber.set(1);

    this.loadClaims();
  }

  previousPage(): void {

    if (this.pageNumber() <= 1 || this.isLoading()) {
      return;
    }

    this.pageNumber.update(page => page - 1);

    this.loadClaims();
  }

  nextPage(): void {

    if (
      this.pageNumber() >= this.totalPages() ||
      this.isLoading()
    ) {
      return;
    }

    this.pageNumber.update(page => page + 1);

    this.loadClaims();
  }

  formatStatus(status: string): string {

    return status
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/\b\w/g, character => character.toUpperCase());
  }

  statusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }

  private updateClaims(result: PagedClaimsResult): void {

    this.claims.set(result.items);

    this.pageNumber.set(result.pageNumber);

    this.totalCount.set(result.totalCount);

    this.totalPages.set(
      Math.max(result.totalPages, 1)
    );
  }
}

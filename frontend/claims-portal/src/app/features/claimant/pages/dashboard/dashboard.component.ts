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
import { ClaimListItem } from '../../../../core/claims/models/claim.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent implements OnInit {

  private readonly claimsApi = inject(ClaimsApiService);

  readonly claims = signal<ClaimListItem[]>([]);
  readonly totalClaims = signal(0);

  readonly isLoading = signal(false);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.claimsApi.getMyClaims(1, 5)
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        })
      )
      .subscribe({

        next: result => {
          this.claims.set(result.items);
          this.totalClaims.set(result.totalCount);
        },

        error: () => {
          this.errorMessage.set(
            'Unable to load your claims. Please try again.'
          );
        }

      });
  }

  formatStatus(status: string): string {

    return status
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/\b\w/g, character => character.toUpperCase());
  }

  statusClass(status: string): string {
    return `status-${status.toLowerCase()}`;
  }
}

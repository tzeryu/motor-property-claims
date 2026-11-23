import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import {
  PagedClaimsResult,
  SubmitClaimRequest,
  SubmitClaimResult,
  ClaimDetails,
  ProvideAdditionalInformationRequest,
  ClaimStatus,
  RequestAdditionalInformationRequest,
  ApproveClaimRequest,
  RejectClaimRequest,
  OfficerApprovalResult,
  ManagementOverview,
  UpdatePaymentStatusRequest
} from '../models/claim.models';

@Injectable({
  providedIn: 'root'
})
export class ClaimsApiService {

  private readonly http = inject(HttpClient);

  private readonly apiUrl =
    `${environment.apiBaseUrl}/claims`;

  getMyClaims(
    pageNumber: number = 1,
    pageSize: number = 20,
    status?: string
  ): Observable<PagedClaimsResult> {

    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (status) {
      params = params.set('status', status);
    }

    return this.http.get<PagedClaimsResult>(
      `${this.apiUrl}/mine`,
      { params }
    );
  }

  submitClaim(
    request: SubmitClaimRequest
  ): Observable<SubmitClaimResult> {

    return this.http.post<SubmitClaimResult>(
      this.apiUrl,
      request
    );
  }

  getClaimById(id: string): Observable<ClaimDetails> {
    return this.http.get<ClaimDetails>(
      `${environment.apiBaseUrl}/claims/${id}`
    );
  }

  provideAdditionalInformation(
    id: string,
    request: ProvideAdditionalInformationRequest
  ): Observable<void> {
    return this.http.post<void>(
      `${environment.apiBaseUrl}/claims/${id}/information-response`,
      request
    );
  }

  getStaffClaims(
    pageNumber: number,
    pageSize: number,
    status?: ClaimStatus,
    assignedToMe = false,
    unassignedOnly = false
  ): Observable<PagedClaimsResult> {

    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    // ASP.NET Core binds enum names, so map the
    // frontend lower-camel-case values to backend names.
    if (status) {
      const statusNames: Record<ClaimStatus, string> = {
        draft: 'Draft',
        submitted: 'Submitted',
        inReview: 'InReview',
        awaitingInformation: 'AwaitingInformation',
        pendingManagerApproval: 'PendingManagerApproval',
        approved: 'Approved',
        rejected: 'Rejected',
        settled: 'Settled'
      };

      params = params.set('status', statusNames[status]);
    }

    if (assignedToMe) {
      params = params.set('assignedToMe', 'true');
    }

    if (unassignedOnly) {
      params = params.set('unassignedOnly', 'true');
    }

    return this.http.get<PagedClaimsResult>(
      `${environment.apiBaseUrl}/claims`,
      { params }
    );
  }

  startReview(id: string): Observable<void> {
    return this.http.post<void>(
      `${environment.apiBaseUrl}/claims/${id}/start-review`,
      null
    );
  }

  requestAdditionalInformation(
    id: string,
    request: RequestAdditionalInformationRequest
  ): Observable<void> {
    return this.http.post<void>(
      `${environment.apiBaseUrl}/claims/${id}/information-request`,
      request
    );
  }

  approveClaim(
    id: string,
    request: ApproveClaimRequest
  ): Observable<OfficerApprovalResult> {
    return this.http.post<OfficerApprovalResult>(
      `${environment.apiBaseUrl}/claims/${id}/approve`,
      request
    );
  }

  rejectClaim(
    id: string,
    request: RejectClaimRequest
  ): Observable<void> {
    return this.http.post<void>(
      `${environment.apiBaseUrl}/claims/${id}/reject`,
      request
    );
  }

  getManagementOverview(): Observable<ManagementOverview> {
    return this.http.get<ManagementOverview>(
      `${environment.apiBaseUrl}/management/overview`
    );
  }

  managerApproveClaim(id: string): Observable<void> {
    return this.http.post<void>(
      `${environment.apiBaseUrl}/claims/${id}/manager-approve`,
      null
    );
  }

  managerRejectClaim(
    id: string,
    request: RejectClaimRequest
  ): Observable<void> {
    return this.http.post<void>(
      `${environment.apiBaseUrl}/claims/${id}/manager-reject`,
      request
    );
  }

  recordSettlement(id: string): Observable<void> {
    return this.http.post<void>(
      `${environment.apiBaseUrl}/claims/${id}/settlement`,
      null
    );
  }

  updatePaymentStatus(
    id: string,
    request: UpdatePaymentStatusRequest
  ): Observable<void> {
    return this.http.put<void>(
      `${environment.apiBaseUrl}/claims/${id}/payment-status`,
      request
    );
  }
}



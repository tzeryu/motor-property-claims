export type ClaimType = 'Motor' | 'Property';

export type ClaimStatus =
  | 'Draft'
  | 'Submitted'
  | 'InReview'
  | 'AwaitingInformation'
  | 'PendingManagerApproval'
  | 'Approved'
  | 'Rejected'
  | 'Settled';

export interface ClaimListItem {
  id: string;
  claimNumber: string;
  claimType: ClaimType;
  status: ClaimStatus;
  claimantName: string;
  policyNumber: string;
  incidentDate: string;
  submittedAtUtc: string;
  assignedOfficerUserId: string | null;
  assignedAtUtc: string | null;
}

export interface PagedClaimsResult {
  items: ClaimListItem[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

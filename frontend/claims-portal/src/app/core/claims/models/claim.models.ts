export type ClaimType = 'motor' | 'property';

export type ClaimStatus =
  | 'draft'
  | 'submitted'
  | 'inReview'
  | 'awaitingInformation'
  | 'pendingManagerApproval'
  | 'approved'
  | 'rejected'
  | 'settled';

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

export interface SubmitClaimRequest {
  claimType: ClaimType;

  claimantName: string;
  claimantEmail: string;
  claimantPhone: string;

  policyDetails: {
    policyNumber: string;
    policyholderName: string;
    coverageStartDate: string;
    coverageEndDate: string;
    insuredAmount: number;
    currencyCode: string;
  };

  incidentDate: string;
  incidentLocation: string;
  incidentDescription: string;

  motorDetails?: {
    vehicleRegistrationNumber: string;
    vehicleMake: string;
    vehicleModel: string;
  };

  propertyDetails?: {
    propertyAddress: string;
    propertyType: string;
    damageDescription: string;
  };
}

export interface SubmitClaimResult {
  id: string;
  claimNumber: string;
  claimType: ClaimType;
  status: ClaimStatus;
}

export interface ClaimPolicyDetails {
  policyNumber: string;
  policyholderName: string;
  coverageStartDate: string;
  coverageEndDate: string;
  insuredAmount: number;
  currencyCode: string;
}

export interface MotorClaimDetails {
  vehicleRegistrationNumber: string;
  vehicleMake: string | null;
  vehicleModel: string | null;
}

export interface PropertyClaimDetails {
  propertyAddress: string;
  propertyType: string;
  damageDescription: string;
}

export type PaymentStatus = 'pending' | 'paid' | 'failed';

export interface ClaimDetails {
  id: string;
  claimNumber: string;
  claimType: ClaimType;
  status: ClaimStatus;

  claimantName: string;
  claimantEmail: string;
  claimantPhone: string;

  policyDetails: ClaimPolicyDetails;

  incidentDate: string;
  incidentLocation: string;
  incidentDescription: string;

  motorDetails: MotorClaimDetails | null;
  propertyDetails: PropertyClaimDetails | null;

  additionalInformationRequest: string | null;
  additionalInformationRequestedAtUtc: string | null;
  additionalInformationResponse: string | null;
  additionalInformationRespondedAtUtc: string | null;

  proposedSettlementAmount: number | null;
  approvedSettlementAmount: number | null;
  rejectionReason: string | null;

  approvalRequestedAtUtc: string | null;
  decisionAtUtc: string | null;
  settlementDateUtc: string | null;

  paymentStatus: PaymentStatus | null;
  paymentReference: string | null;
  paymentRecordedAtUtc: string | null;

  createdAtUtc: string;
  submittedAtUtc: string | null;
}

export interface ProvideAdditionalInformationRequest {
  response: string;
}

export interface RequestAdditionalInformationRequest {
  message: string;
}

export interface ApproveClaimRequest {
  settlementAmount: number;
}

export interface RejectClaimRequest {
  reason: string;
}

export interface OfficerApprovalResult {
  status: ClaimStatus;
  managerApprovalRequired: boolean;
}

export interface CurrencyExposure {
  currencyCode: string;
  pendingApprovalExposure: number;
  approvedUnpaidLiability: number;
  settledAmount: number;
  totalOutstandingExposure: number;
}

export interface ManagementOverview {
  totalOutstandingClaims: number;
  submittedClaims: number;
  inReviewClaims: number;
  awaitingInformationClaims: number;
  pendingManagerApprovalClaims: number;
  approvedUnpaidClaims: number;
  settledClaims: number;
  rejectedClaims: number;
  liabilityByCurrency: CurrencyExposure[];
}

export type PaymentOutcome = 'paid' | 'failed';

export interface UpdatePaymentStatusRequest {
  status: PaymentOutcome;
  paymentReference: string | null;
}



using Claims.Application.Claims.DTOs.Queries;
using Claims.Application.Claims.Interfaces;
using Claims.Domain.Entities;

namespace Claims.Application.Claims.Queries.GetClaimById;

public sealed class GetClaimByIdHandler
{
    private readonly IClaimRepository _claimRepository;

    public GetClaimByIdHandler(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }

    public async Task<ClaimDetailsResult?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var claim = await _claimRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (claim is null)
        {
            return null;
        }

        return MapToResult(claim);
    }

    private static ClaimDetailsResult MapToResult(Claim claim)
    {
        var policy = claim.PolicyDetails;

        return new ClaimDetailsResult(
            Id: claim.Id,
            ClaimNumber: claim.ClaimNumber,
            ClaimType: claim.ClaimType,
            Status: claim.Status,

            ClaimantName: claim.ClaimantName,
            ClaimantEmail: claim.ClaimantEmail,
            ClaimantPhone: claim.ClaimantPhone,

            PolicyDetails: new PolicyDetailsResult(
                policy.PolicyNumber,
                policy.PolicyholderName,
                policy.CoverageStartDate,
                policy.CoverageEndDate,
                policy.InsuredAmount,
                policy.CurrencyCode
            ),

            IncidentDate: claim.IncidentDate,
            IncidentLocation: claim.IncidentLocation,
            IncidentDescription: claim.IncidentDescription,

            MotorDetails: claim.MotorDetails is null
                ? null
                : new MotorClaimDetailsResult(
                    claim.MotorDetails.VehicleRegistrationNumber,
                    claim.MotorDetails.VehicleMake,
                    claim.MotorDetails.VehicleModel
                ),

            PropertyDetails: claim.PropertyDetails is null
                ? null
                : new PropertyClaimDetailsResult(
                    claim.PropertyDetails.PropertyAddress,
                    claim.PropertyDetails.PropertyType,
                    claim.PropertyDetails.DamageDescription
                ),

            AdditionalInformationRequest:
    claim.AdditionalInformationRequest,

AdditionalInformationRequestedAtUtc:
    claim.AdditionalInformationRequestedAtUtc,

AdditionalInformationResponse:
    claim.AdditionalInformationResponse,

AdditionalInformationRespondedAtUtc:
    claim.AdditionalInformationRespondedAtUtc,

ProposedSettlementAmount:
    claim.ProposedSettlementAmount,

ApprovedSettlementAmount:
    claim.ApprovedSettlementAmount,

RejectionReason:
    claim.RejectionReason,

ApprovalRequestedAtUtc:
    claim.ApprovalRequestedAtUtc,

DecisionAtUtc:
    claim.DecisionAtUtc,

SettlementDateUtc: claim.SettlementDateUtc,
PaymentStatus: claim.PaymentStatus,
PaymentReference: claim.PaymentReference,
PaymentRecordedAtUtc: claim.PaymentRecordedAtUtc,

            CreatedAtUtc: claim.CreatedAtUtc,
            SubmittedAtUtc: claim.SubmittedAtUtc
        );
    }
}


using Claims.Domain.Enums;
using Claims.Domain.ValueObjects;
using PaymentStatusEnum = Claims.Domain.Enums.PaymentStatus;

namespace Claims.Domain.Entities;

public sealed class Claim
{
    private Claim()
    {
        // Required by Entity Framework Core.
    }

    private Claim(
        string claimNumber,
        ClaimType claimType,
        string claimantName,
        string claimantEmail,
        string claimantPhone,
        Guid submittedByUserId,
        PolicyDetails policyDetails,
        DateOnly incidentDate,
        string incidentLocation,
        string incidentDescription,
        DateTimeOffset submittedAtUtc,
        MotorClaimDetails? motorDetails,
        PropertyClaimDetails? propertyDetails)
    {
        Id = Guid.NewGuid();

        ClaimNumber = claimNumber;
        ClaimType = claimType;
        Status = ClaimStatus.Submitted;

        ClaimantName = claimantName;
        ClaimantEmail = claimantEmail;
        ClaimantPhone = claimantPhone;
        SubmittedByUserId = submittedByUserId;

        PolicyDetails = policyDetails;

        IncidentDate = incidentDate;
        IncidentLocation = incidentLocation;
        IncidentDescription = incidentDescription;

        CreatedAtUtc = submittedAtUtc;
        SubmittedAtUtc = submittedAtUtc;

        MotorDetails = motorDetails;
        PropertyDetails = propertyDetails;
    }

    public Guid Id { get; private set; }

    public Guid? SubmittedByUserId { get; private set; }

    public string ClaimNumber { get; private set; } = string.Empty;

    public ClaimType ClaimType { get; private set; }

    public ClaimStatus Status { get; private set; }

    // Claimant information
    public string ClaimantName { get; private set; } = string.Empty;

    public string ClaimantEmail { get; private set; } = string.Empty;

    public string ClaimantPhone { get; private set; } = string.Empty;

    // Policy information
    public PolicyDetails PolicyDetails { get; private set; } = null!;

    // Incident information
    public DateOnly IncidentDate { get; private set; }

    public string IncidentLocation { get; private set; } = string.Empty;

    public string IncidentDescription { get; private set; } = string.Empty;

    // Claim-specific information
    public MotorClaimDetails? MotorDetails { get; private set; }

    public PropertyClaimDetails? PropertyDetails { get; private set; }

    // Timestamps
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset SubmittedAtUtc { get; private set; }

    // Latest review information
    public Guid? LastReviewedByUserId { get; private set; }

    public DateTimeOffset? LastReviewedAtUtc { get; private set; }

    // Latest additional-information request
    public string? AdditionalInformationRequest { get; private set; }

    public Guid? AdditionalInformationRequestedByUserId { get; private set; }

    public DateTimeOffset? AdditionalInformationRequestedAtUtc { get; private set; }

    // Latest claimant response
    public string? AdditionalInformationResponse { get; private set; }

    public DateTimeOffset? AdditionalInformationRespondedAtUtc { get; private set; }

    // Proposed and approved settlement amounts
    public decimal? ProposedSettlementAmount { get; private set; }

    public decimal? ApprovedSettlementAmount { get; private set; }

    // Manager escalation
    public Guid? ApprovalRequestedByUserId { get; private set; }

    public DateTimeOffset? ApprovalRequestedAtUtc { get; private set; }

    // Final decision
    public Guid? DecisionByUserId { get; private set; }

    public DateTimeOffset? DecisionAtUtc { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTimeOffset? SettlementDateUtc { get; private set; }

    public string? PaymentReference { get; private set; }

    public DateTimeOffset? PaymentRecordedAtUtc { get; private set; }

    public PaymentStatusEnum? PaymentStatus { get; private set; }

    // Factory method for motor claims
    public static Claim SubmitMotor(
        string claimNumber,
        string claimantName,
        string claimantEmail,
        string claimantPhone,
        Guid submittedByUserId,
        PolicyDetails policyDetails,
        DateOnly incidentDate,
        string incidentLocation,
        string incidentDescription,
        MotorClaimDetails motorDetails,
        DateTimeOffset submittedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(motorDetails);

        if (submittedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid submitting user ID is required.",
                nameof(submittedByUserId)
            );
        }

        return new Claim(
            claimNumber,
            ClaimType.Motor,
            claimantName,
            claimantEmail,
            claimantPhone,
            submittedByUserId,
            policyDetails,
            incidentDate,
            incidentLocation,
            incidentDescription,
            submittedAtUtc,
            motorDetails,
            null);
    }

    // Factory method for property claims
    public static Claim SubmitProperty(
        string claimNumber,
        string claimantName,
        string claimantEmail,
        string claimantPhone,
        Guid submittedByUserId,
        PolicyDetails policyDetails,
        DateOnly incidentDate,
        string incidentLocation,
        string incidentDescription,
        PropertyClaimDetails propertyDetails,
        DateTimeOffset submittedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(propertyDetails);

        if (submittedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid submitting user ID is required.",
                nameof(submittedByUserId)
            );
        }

        return new Claim(
            claimNumber,
            ClaimType.Property,
            claimantName,
            claimantEmail,
            claimantPhone,
            submittedByUserId,
            policyDetails,
            incidentDate,
            incidentLocation,
            incidentDescription,
            submittedAtUtc,
            null,
            propertyDetails);
    }

    public void StartReview(
    Guid reviewerUserId,
    DateTimeOffset reviewedAtUtc)
    {
        if (reviewerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid reviewer ID is required.",
                nameof(reviewerUserId)
            );
        }

        if (Status != ClaimStatus.Submitted)
        {
            throw new InvalidOperationException(
                "Only submitted claims can enter review."
            );
        }

        LastReviewedByUserId = reviewerUserId;
        LastReviewedAtUtc = reviewedAtUtc;

        Status = ClaimStatus.InReview;
    }

    public void RequestAdditionalInformation(
    Guid requesterUserId,
    string message,
    DateTimeOffset requestedAtUtc)
    {
        if (requesterUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid requester ID is required.",
                nameof(requesterUserId)
            );
        }

        if (Status != ClaimStatus.InReview)
        {
            throw new InvalidOperationException(
                "Additional information can only be requested during review."
            );
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "The information request message is required.",
                nameof(message)
            );
        }

        if (message.Trim().Length > 2000)
        {
            throw new ArgumentException(
                "The information request cannot exceed 2000 characters.",
                nameof(message)
            );
        }

        AdditionalInformationRequest = message.Trim();
        AdditionalInformationRequestedByUserId = requesterUserId;
        AdditionalInformationRequestedAtUtc = requestedAtUtc;

        // Clear the previous response when making a new request.
        AdditionalInformationResponse = null;
        AdditionalInformationRespondedAtUtc = null;

        Status = ClaimStatus.AwaitingInformation;
    }

    public void ProvideAdditionalInformation(
    Guid claimantUserId,
    string response,
    DateTimeOffset respondedAtUtc)
    {
        if (SubmittedByUserId != claimantUserId)
        {
            throw new InvalidOperationException(
                "Only the claim owner can provide additional information."
            );
        }

        if (Status != ClaimStatus.AwaitingInformation)
        {
            throw new InvalidOperationException(
                "This claim is not awaiting additional information."
            );
        }

        if (string.IsNullOrWhiteSpace(response))
        {
            throw new ArgumentException(
                "The additional information response is required.",
                nameof(response)
            );
        }

        if (response.Trim().Length > 4000)
        {
            throw new ArgumentException(
                "The response cannot exceed 4000 characters.",
                nameof(response)
            );
        }

        AdditionalInformationResponse = response.Trim();
        AdditionalInformationRespondedAtUtc = respondedAtUtc;

        Status = ClaimStatus.InReview;
    }

    public bool SubmitOfficerApproval(
    Guid officerUserId,
    decimal proposedAmount,
    decimal officerApprovalLimit,
    string authorityCurrencyCode,
    DateTimeOffset submittedAtUtc)
    {
        if (officerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid officer ID is required.",
                nameof(officerUserId)
            );
        }

        if (Status != ClaimStatus.InReview)
        {
            throw new InvalidOperationException(
                "Only claims under review can be approved."
            );
        }

        if (proposedAmount <= 0)
        {
            throw new ArgumentException(
                "The proposed settlement amount must be greater than zero."
            );
        }

        if (officerApprovalLimit <= 0)
        {
            throw new ArgumentException(
                "The officer approval limit must be greater than zero."
            );
        }

        if (string.IsNullOrWhiteSpace(authorityCurrencyCode))
        {
            throw new ArgumentException(
                "The authority currency is required."
            );
        }

        ProposedSettlementAmount = proposedAmount;

        var requiresManagerApproval =
            !string.Equals(
                PolicyDetails.CurrencyCode,
                authorityCurrencyCode,
                StringComparison.OrdinalIgnoreCase
            )
            || proposedAmount > officerApprovalLimit;

        if (requiresManagerApproval)
        {
            ApprovalRequestedByUserId = officerUserId;
            ApprovalRequestedAtUtc = submittedAtUtc;

            Status = ClaimStatus.PendingManagerApproval;

            return true;
        }

        // Officer has sufficient approval authority.
        ApprovedSettlementAmount = proposedAmount;
        DecisionByUserId = officerUserId;
        DecisionAtUtc = submittedAtUtc;

        Status = ClaimStatus.Approved;

        return false;
    }

    public void RejectByClaimsOfficer(
    Guid officerUserId,
    string reason,
    DateTimeOffset rejectedAtUtc)
    {
        if (officerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid officer ID is required."
            );
        }

        if (Status != ClaimStatus.InReview)
        {
            throw new InvalidOperationException(
                "Only claims under review can be rejected by an officer."
            );
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "A rejection reason is required."
            );
        }

        RejectionReason = reason.Trim();
        DecisionByUserId = officerUserId;
        DecisionAtUtc = rejectedAtUtc;

        Status = ClaimStatus.Rejected;
    }

    public void ApproveByManager(
    Guid managerUserId,
    DateTimeOffset approvedAtUtc)
    {
        if (managerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid manager ID is required."
            );
        }

        if (Status != ClaimStatus.PendingManagerApproval)
        {
            throw new InvalidOperationException(
                "This claim is not pending manager approval."
            );
        }

        if (!ProposedSettlementAmount.HasValue ||
            ProposedSettlementAmount.Value <= 0)
        {
            throw new InvalidOperationException(
                "A valid proposed settlement amount is required."
            );
        }

        ApprovedSettlementAmount = ProposedSettlementAmount.Value;
        DecisionByUserId = managerUserId;
        DecisionAtUtc = approvedAtUtc;

        Status = ClaimStatus.Approved;
    }

    public void RejectByManager(
    Guid managerUserId,
    string reason,
    DateTimeOffset rejectedAtUtc)
    {
        if (managerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid manager ID is required."
            );
        }

        if (Status != ClaimStatus.PendingManagerApproval)
        {
            throw new InvalidOperationException(
                "This claim is not pending manager approval."
            );
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "A rejection reason is required."
            );
        }

        RejectionReason = reason.Trim();
        DecisionByUserId = managerUserId;
        DecisionAtUtc = rejectedAtUtc;

        Status = ClaimStatus.Rejected;
    }

    public void RecordSettlement(DateTimeOffset settlementDateUtc)
    {
        if (Status != ClaimStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved claims can be recorded for settlement."
            );
        }

        if (!ApprovedSettlementAmount.HasValue ||
            ApprovedSettlementAmount.Value <= 0)
        {
            throw new InvalidOperationException(
                "A valid approved settlement amount is required."
            );
        }

        if (SettlementDateUtc.HasValue)
        {
            throw new InvalidOperationException(
                "Settlement has already been recorded."
            );
        }

        SettlementDateUtc = settlementDateUtc;
        PaymentStatus = PaymentStatusEnum.Pending;
    }

    public void RecordPaymentOutcome(
    PaymentStatusEnum paymentStatus,
    string? paymentReference,
    DateTimeOffset recordedAtUtc)
    {
        if (!SettlementDateUtc.HasValue ||
            !PaymentStatus.HasValue)
        {
            throw new InvalidOperationException(
                "Settlement must be recorded before updating payment status."
            );
        }

        if (PaymentStatus == PaymentStatusEnum.Paid)
        {
            throw new InvalidOperationException(
                "A completed payment cannot be changed."
            );
        }

        if (paymentStatus != PaymentStatusEnum.Paid &&
            paymentStatus != PaymentStatusEnum.Failed)
        {
            throw new ArgumentException(
                "Payment status must be Paid or Failed."
            );
        }

        var reference = string.IsNullOrWhiteSpace(paymentReference)
            ? null
            : paymentReference.Trim();

        if (reference?.Length > 100)
        {
            throw new ArgumentException(
                "Payment reference cannot exceed 100 characters."
            );
        }

        if (paymentStatus == PaymentStatusEnum.Paid &&
            reference is null)
        {
            throw new ArgumentException(
                "A payment reference is required for successful payment."
            );
        }

        PaymentStatus = paymentStatus;
        PaymentReference = reference;
        PaymentRecordedAtUtc = recordedAtUtc;

        if (paymentStatus == PaymentStatusEnum.Paid)
        {
            Status = ClaimStatus.Settled;
        }
    }
}

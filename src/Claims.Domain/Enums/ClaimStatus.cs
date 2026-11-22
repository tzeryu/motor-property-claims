
namespace Claims.Domain.Enums;

public enum ClaimStatus
{
    Draft = 1,
    Submitted = 2,
    InReview = 3,
    AwaitingInformation = 4,
    PendingManagerApproval = 5,
    Approved = 6,
    Rejected = 7,
    Settled = 8
}

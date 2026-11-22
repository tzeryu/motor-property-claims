using System.ComponentModel.DataAnnotations;
using Claims.Domain.Enums;

namespace Claims.Application.Claims.DTOs.Workflow;

public sealed class UpdatePaymentStatusRequest : IValidatableObject
{
    [EnumDataType(typeof(PaymentStatus))]
    public PaymentStatus Status { get; init; }

    [StringLength(100)]
    public string? PaymentReference { get; init; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Status == PaymentStatus.Pending ||
            !Enum.IsDefined(Status))
        {
            yield return new ValidationResult(
                "Payment status must be Paid or Failed.",
                [nameof(Status)]
            );
        }

        if (Status == PaymentStatus.Paid &&
            string.IsNullOrWhiteSpace(PaymentReference))
        {
            yield return new ValidationResult(
                "Payment reference is required for successful payment.",
                [nameof(PaymentReference)]
            );
        }
    }
}
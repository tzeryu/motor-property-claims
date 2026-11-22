using System.ComponentModel.DataAnnotations;

namespace Claims.Application.Claims.DTOs.Workflow;

public sealed class RejectClaimRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Reason { get; init; } = string.Empty;
}
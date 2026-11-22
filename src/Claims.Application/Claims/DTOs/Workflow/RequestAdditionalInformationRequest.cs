using System.ComponentModel.DataAnnotations;

namespace Claims.Application.Claims.DTOs.Workflow;

public sealed class RequestAdditionalInformationRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Message { get; init; } = string.Empty;
}

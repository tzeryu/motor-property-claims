using System.ComponentModel.DataAnnotations;

namespace Claims.Application.Claims.DTOs.Workflow;

public sealed class ProvideAdditionalInformationRequest
{
    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Response { get; init; } = string.Empty;
}
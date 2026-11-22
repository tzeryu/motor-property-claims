
using System.ComponentModel.DataAnnotations;

namespace Claims.Application.Claims.DTOs.Submission;

public sealed class PropertyClaimDetailsRequest
{
    [Required(ErrorMessage = "Property address is required.")]
    [StringLength(500)]
    public string? PropertyAddress { get; init; }

    [Required(ErrorMessage = "Property type is required.")]
    [StringLength(100)]
    public string? PropertyType { get; init; }

    [Required(ErrorMessage = "Damage description is required.")]
    [StringLength(2000)]
    public string? DamageDescription { get; init; }
}

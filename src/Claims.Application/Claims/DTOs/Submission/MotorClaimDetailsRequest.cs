
using System.ComponentModel.DataAnnotations;

namespace Claims.Application.Claims.DTOs.Submission;

public sealed class MotorClaimDetailsRequest
{
    [Required(ErrorMessage = "Vehicle registration number is required.")]
    [StringLength(20)]
    public string? VehicleRegistrationNumber { get; init; }

    [StringLength(100)]
    public string? VehicleMake { get; init; }

    [StringLength(100)]
    public string? VehicleModel { get; init; }
}

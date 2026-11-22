
namespace Claims.Domain.Entities;

public sealed class MotorClaimDetails
{
    private MotorClaimDetails()
    {
        // Required by Entity Framework Core.
    }

    public MotorClaimDetails(
        string vehicleRegistrationNumber,
        string? vehicleMake,
        string? vehicleModel)
    {
        VehicleRegistrationNumber = vehicleRegistrationNumber;
        VehicleMake = vehicleMake;
        VehicleModel = vehicleModel;
    }

    public Guid ClaimId { get; private set; }

    public string VehicleRegistrationNumber { get; private set; }
        = string.Empty;

    public string? VehicleMake { get; private set; }

    public string? VehicleModel { get; private set; }

    public Claim Claim { get; private set; } = null!;
}

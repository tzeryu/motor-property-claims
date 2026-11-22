
namespace Claims.Domain.Entities;

public sealed class PropertyClaimDetails
{
    private PropertyClaimDetails()
    {
        // Required by Entity Framework Core.
    }

    public PropertyClaimDetails(
        string propertyAddress,
        string propertyType,
        string damageDescription)
    {
        PropertyAddress = propertyAddress;
        PropertyType = propertyType;
        DamageDescription = damageDescription;
    }

    public Guid ClaimId { get; private set; }

    public string PropertyAddress { get; private set; }
        = string.Empty;

    public string PropertyType { get; private set; }
        = string.Empty;

    public string DamageDescription { get; private set; }
        = string.Empty;

    public Claim Claim { get; private set; } = null!;
}


using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Infrastructure.Persistence.Configurations;

public class MotorClaimDetailsConfiguration
    : IEntityTypeConfiguration<MotorClaimDetails>
{
    public void Configure(
        EntityTypeBuilder<MotorClaimDetails> builder)
    {
        builder.ToTable("MotorClaimDetails");

        builder.HasKey(m => m.ClaimId);

        builder.Property(m => m.VehicleRegistrationNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.VehicleMake)
            .HasMaxLength(100);

        builder.Property(m => m.VehicleModel)
            .HasMaxLength(100);
    }
}


using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Infrastructure.Persistence.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");

        // Primary key
        builder.HasKey(c => c.Id);

        // Claim identification
        builder.Property(c => c.ClaimNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(c => c.ClaimNumber)
            .IsUnique();

        builder.Property(c => c.ClaimType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(c => c.Status);

        // Claimant information
        builder.Property(c => c.ClaimantName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.ClaimantEmail)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(c => c.ClaimantPhone)
            .IsRequired()
            .HasMaxLength(30);

        // Incident information
        builder.Property(c => c.IncidentDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(c => c.IncidentLocation)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.IncidentDescription)
            .IsRequired()
            .HasMaxLength(2000);

        // Timestamps
        builder.Property(c => c.CreatedAtUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(c => c.SubmittedAtUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        // PolicyDetails: owned value object
        builder.OwnsOne(c => c.PolicyDetails, policy =>
        {
            policy.Property(p => p.PolicyNumber)
                .HasColumnName("PolicyNumber")
                .IsRequired()
                .HasMaxLength(50);

            policy.Property(p => p.PolicyholderName)
                .HasColumnName("PolicyholderName")
                .IsRequired()
                .HasMaxLength(150);

            policy.Property(p => p.CoverageStartDate)
                .HasColumnName("CoverageStartDate")
                .HasColumnType("date")
                .IsRequired();

            policy.Property(p => p.CoverageEndDate)
                .HasColumnName("CoverageEndDate")
                .HasColumnType("date")
                .IsRequired();

            policy.Property(p => p.InsuredAmount)
                .HasColumnName("InsuredAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            policy.Property(p => p.CurrencyCode)
                .HasColumnName("CurrencyCode")
                .IsRequired()
                .HasMaxLength(3);
        });

        builder.Navigation(c => c.PolicyDetails)
            .IsRequired();

        // Optional one-to-one: Motor claim details
        builder.HasOne(c => c.MotorDetails)
            .WithOne(m => m.Claim)
            .HasForeignKey<MotorClaimDetails>(m => m.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optional one-to-one: Property claim details
        builder.HasOne(c => c.PropertyDetails)
            .WithOne(p => p.Claim)
            .HasForeignKey<PropertyClaimDetails>(p => p.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

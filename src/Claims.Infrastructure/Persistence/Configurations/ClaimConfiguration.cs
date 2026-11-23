
using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Claims.Infrastructure.Identity;


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

        builder.Property(c => c.SubmittedByUserId)
            .IsRequired(false);

        builder.Property(c => c.LastReviewedAtUtc)
    .HasColumnType("datetimeoffset");

        builder.Property(c => c.AdditionalInformationRequest)
            .HasMaxLength(2000);

        builder.Property(c => c.AdditionalInformationRequestedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.Property(c => c.AdditionalInformationResponse)
            .HasMaxLength(4000);

        builder.Property(c => c.AdditionalInformationRespondedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.Property(c => c.ProposedSettlementAmount)
    .HasPrecision(18, 2);

        builder.Property(c => c.ApprovedSettlementAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.ApprovalRequestedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.Property(c => c.DecisionAtUtc)
            .HasColumnType("datetimeoffset");

        builder.Property(c => c.RejectionReason)
            .HasMaxLength(2000);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.ApprovalRequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.DecisionByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.SettlementDateUtc)
    .HasColumnType("datetimeoffset");

        builder.Property(c => c.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.PaymentReference)
            .HasMaxLength(100);

        builder.Property(c => c.PaymentRecordedAtUtc)
            .HasColumnType("datetimeoffset");

        // Reviewer must reference an existing Identity user.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.LastReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Information requester must reference an existing Identity user.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.AdditionalInformationRequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.SubmittedByUserId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

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

        builder.Property(c => c.AssignedAtUtc)
    .HasColumnType("datetimeoffset");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.AssignedOfficerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Optimistic concurrency protection.
        // Prevents two officers from picking up the same claim simultaneously.
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
    }
}

using System.ComponentModel.DataAnnotations;
using Claims.Application.Claims.DTOs.Submission;
using Claims.Domain.Enums;

namespace Claims.UnitTests.Submission;

public class SubmitClaimRequestTests
{
    [Fact]
    public void Validate_MotorClaimWithoutMotorDetails_ReturnsError()
    {
        // Arrange
        var request = new SubmitClaimRequest
        {
            ClaimType = ClaimType.Motor,
            MotorDetails = null
        };

        // Act
        var results = request.Validate(
            new ValidationContext(request)).ToList();

        // Assert
        Assert.Contains(results, result =>
            result.MemberNames.Contains(
                nameof(SubmitClaimRequest.MotorDetails)));
    }

    [Fact]
    public void Validate_PropertyClaimWithMotorDetails_ReturnsError()
    {
        // Arrange
        var request = new SubmitClaimRequest
        {
            ClaimType = ClaimType.Property,
            MotorDetails = new MotorClaimDetailsRequest
            {
                VehicleRegistrationNumber = "ABC1234"
            }
        };

        // Act
        var results = request.Validate(
            new ValidationContext(request)).ToList();

        // Assert
        Assert.Contains(results, result =>
            result.MemberNames.Contains(
                nameof(SubmitClaimRequest.MotorDetails)));
    }
}
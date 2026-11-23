using System.Net;
using System.Net.Http.Json;

using Claims.Application.Common.Authorization;
using Claims.IntegrationTests.Infrastructure;

using Xunit;

namespace Claims.IntegrationTests.Claims;

public class ClaimAssignmentTests
{
    [Fact]
    public async Task FirstOfficerCanPickUpClaim_SecondOfficerCannotTakeIt()
    {
        using var factory = new ClaimsApiFactory();

        try
        {
            await factory.InitializeDatabaseAsync();

            // Claimant submits a new claim.
            using var claimant = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestClaimantUserId,
                RoleNames.Claimant
            );

            var submission = await claimant.PostAsJsonAsync(
                "/api/v1/claims",
                CreateMotorClaimRequest()
            );

            Assert.Equal(HttpStatusCode.Created, submission.StatusCode);

            var submitted = await submission.Content
                .ReadFromJsonAsync<SubmitClaimTestResult>();

            Assert.NotNull(submitted);

            // Officer A picks up the claim by starting review.
            using var officerA = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOfficerUserId,
                RoleNames.ClaimsOfficer
            );

            var firstReview = await officerA.PostAsync(
                $"/api/v1/claims/{submitted.Id}/start-review",
                null
            );

            Assert.Equal(HttpStatusCode.NoContent, firstReview.StatusCode);

            // Officer B attempts to take the same claim.
            using var officerB = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOtherClaimantUserId,
                RoleNames.ClaimsOfficer
            );

            var secondReview = await officerB.PostAsync(
                $"/api/v1/claims/{submitted.Id}/start-review",
                null
            );

            Assert.Equal(HttpStatusCode.Conflict, secondReview.StatusCode);

            // Officer A can see the claim in their work queue.
            var myClaims = await officerA.GetAsync(
                "/api/v1/claims?assignedToMe=true"
            );

            Assert.Equal(HttpStatusCode.OK, myClaims.StatusCode);
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    private static object CreateMotorClaimRequest()
    {
        return new
        {
            claimType = "motor",
            claimantName = "Test Claimant",
            claimantEmail = "test@example.com",
            claimantPhone = "0123456789",

            policyDetails = new
            {
                policyNumber = "POL-ASSIGN-001",
                policyholderName = "Test Claimant",
                coverageStartDate = "2026-01-01",
                coverageEndDate = "2026-12-31",
                insuredAmount = 50000m,
                currencyCode = "MYR"
            },

            incidentDate = "2026-11-20",
            incidentLocation = "Kuala Lumpur",
            incidentDescription = "Vehicle collision.",

            motorDetails = new
            {
                vehicleRegistrationNumber = "ABC1234",
                vehicleMake = "Toyota",
                vehicleModel = "Vios"
            }
        };
    }

    private sealed record SubmitClaimTestResult(Guid Id);
}
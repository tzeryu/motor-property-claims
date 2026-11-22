using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Claims.Application.Common.Authorization;
using Claims.IntegrationTests.Infrastructure;

using Xunit;

namespace Claims.IntegrationTests.Claims;

public class ClaimListingTests
{
    [Fact]
    public async Task Claimant_ListsOwnClaims_StaffListsAllClaims()
    {
        using var factory = new ClaimsApiFactory();

        await factory.InitializeDatabaseAsync();

        try
        {
            // Claimant A submits a claim.
            using var claimantA = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestClaimantUserId,
                RoleNames.Claimant
            );

            var responseA = await claimantA.PostAsJsonAsync(
                "/api/v1/claims",
                CreateMotorClaimRequest("POL-LIST-001")
            );

            Assert.Equal(HttpStatusCode.Created, responseA.StatusCode);

            // Claimant B submits another claim.
            using var claimantB = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOtherClaimantUserId,
                RoleNames.Claimant
            );

            var responseB = await claimantB.PostAsJsonAsync(
                "/api/v1/claims",
                CreateMotorClaimRequest("POL-LIST-002")
            );

            Assert.Equal(HttpStatusCode.Created, responseB.StatusCode);

            // Claimant A retrieves their own list.
            var myClaimsResponse = await claimantA.GetAsync(
                "/api/v1/claims/mine?pageNumber=1&pageSize=10"
            );

            Assert.Equal(HttpStatusCode.OK, myClaimsResponse.StatusCode);

            using var myClaimsJson = JsonDocument.Parse(
                await myClaimsResponse.Content.ReadAsStringAsync()
            );

            var myClaims = myClaimsJson.RootElement
                .GetProperty("items");

            Assert.Single(myClaims.EnumerateArray());

            Assert.Equal(
                1,
                myClaimsJson.RootElement.GetProperty("totalCount").GetInt32()
            );

            // Staff retrieves all claims.
            using var officer = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOfficerUserId,
                RoleNames.ClaimsOfficer
            );

            var staffResponse = await officer.GetAsync(
                "/api/v1/claims?pageNumber=1&pageSize=10"
            );

            Assert.Equal(HttpStatusCode.OK, staffResponse.StatusCode);

            using var staffJson = JsonDocument.Parse(
                await staffResponse.Content.ReadAsStringAsync()
            );

            var staffClaims = staffJson.RootElement
                .GetProperty("items");

            Assert.Equal(2, staffClaims.GetArrayLength());

            Assert.Equal(
                2,
                staffJson.RootElement.GetProperty("totalCount").GetInt32()
            );
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    private static object CreateMotorClaimRequest(string policyNumber)
    {
        return new
        {
            claimType = "motor",
            claimantName = "Test Claimant",
            claimantEmail = "test@example.com",
            claimantPhone = "0123456789",

            policyDetails = new
            {
                policyNumber,
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
}
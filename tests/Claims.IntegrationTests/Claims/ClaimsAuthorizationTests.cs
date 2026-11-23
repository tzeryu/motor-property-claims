using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Claims.Application.Common.Authorization;
using Claims.IntegrationTests.Infrastructure;

namespace Claims.IntegrationTests.Claims;

public class ClaimsAuthorizationTests
{
    [Fact]
    public async Task GetClaim_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new ClaimsApiFactory();

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/v1/claims/{Guid.NewGuid()}");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetClaim_AsOwnerClaimant_ReturnsOk()
    {
        using var factory = new ClaimsApiFactory();

        await factory.InitializeDatabaseAsync();

        using var claimantClient =
            factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestClaimantUserId,
                RoleNames.Claimant);

        var claimId = await SubmitClaimAsync(claimantClient);

        var response = await claimantClient.GetAsync(
            $"/api/v1/claims/{claimId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task GetClaim_AsDifferentClaimant_ReturnsNotFound()
    {
        using var factory = new ClaimsApiFactory();

        await factory.InitializeDatabaseAsync();

        using var ownerClient =
            factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestClaimantUserId,
                RoleNames.Claimant);

        var claimId = await SubmitClaimAsync(ownerClient);

        using var otherClaimantClient =
            factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOtherClaimantUserId,
                RoleNames.Claimant);

        var response = await otherClaimantClient.GetAsync(
            $"/api/v1/claims/{claimId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetClaim_AsClaimsOfficer_ReturnsOk()
    {
        using var factory = new ClaimsApiFactory();

        await factory.InitializeDatabaseAsync();

        using var claimantClient =
            factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestClaimantUserId,
                RoleNames.Claimant);

        var claimId = await SubmitClaimAsync(claimantClient);

        using var officerClient =
            factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOfficerUserId,
                RoleNames.ClaimsOfficer);

        var response = await officerClient.GetAsync(
            $"/api/v1/claims/{claimId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task GetClaim_AsClaimant_UnknownClaim_ReturnsNotFound()
    {
        using var factory = new ClaimsApiFactory();

        await factory.InitializeDatabaseAsync();

        using var client =
            factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestClaimantUserId,
                RoleNames.Claimant);

        var response = await client.GetAsync(
            $"/api/v1/claims/{Guid.NewGuid()}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private static async Task<Guid> SubmitClaimAsync(
        HttpClient client)
    {
        var request = new
        {
            claimType = "motor",

            claimantName = "Test Claimant",
            claimantEmail = "claimant-test@example.com",
            claimantPhone = "0123456789",

            policyDetails = new
            {
                policyNumber = "POL-TEST-001",
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

        var response = await client.PostAsJsonAsync(
            "/api/v1/claims",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        return json.RootElement
            .GetProperty("id")
            .GetGuid();
    }
}
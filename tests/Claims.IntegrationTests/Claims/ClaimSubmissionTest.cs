
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Claims.IntegrationTests.Infrastructure;
using Xunit;

namespace Claims.IntegrationTests.Claims;

public class ClaimSubmissionTests
{
    [Fact]
    public Task SubmitMotorClaim_ThenRetrieve_ReturnsPersistedClaim()
    {
        return WithTestClientAsync(async client =>
        {
            // Arrange
            var request = CreateMotorClaimRequest();

            // Act 1: Submit claim
            var postResponse = await client.PostAsJsonAsync(
                "/api/v1/claims",
                request);

            // Assert 1: Submission successful
            Assert.Equal(
                HttpStatusCode.Created,
                postResponse.StatusCode);

            Assert.NotNull(postResponse.Headers.Location);

            var submittedJson = JsonDocument.Parse(
                await postResponse.Content.ReadAsStringAsync());

            var submitted = submittedJson.RootElement;

            var claimId = submitted.GetProperty("id").GetGuid();

            var claimNumber = submitted
                .GetProperty("claimNumber").GetString();

            Assert.False(string.IsNullOrWhiteSpace(claimNumber));

            Assert.Equal(
                "motor",
                submitted.GetProperty("claimType").GetString());

            Assert.Equal(
                "submitted",
                submitted.GetProperty("status").GetString());

            // Act 2: Retrieve the saved claim
            var getResponse = await client.GetAsync(
                $"/api/v1/claims/{claimId}");

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            var retrievedJson = JsonDocument.Parse(
                await getResponse.Content.ReadAsStringAsync());

            var claim = retrievedJson.RootElement;

            // Assert 2: Verify persisted claim
            Assert.Equal(
                claimId,
                claim.GetProperty("id").GetGuid());

            Assert.Equal(
                claimNumber,
                claim.GetProperty("claimNumber").GetString());

            Assert.Equal(
                "John Tan",
                claim.GetProperty("claimantName").GetString());

            Assert.Equal(
                "POL-2026-001",
                claim.GetProperty("policyDetails")
                    .GetProperty("policyNumber").GetString());

            Assert.Equal(
                "ABC1234",
                claim.GetProperty("motorDetails")
                    .GetProperty("vehicleRegistrationNumber").GetString());

            Assert.Equal(
                JsonValueKind.Null,
                claim.GetProperty("propertyDetails").ValueKind);
        });
    }

    [Fact]
    public Task SubmitPropertyClaim_ThenRetrieve_ReturnsPersistedClaim()
    {
        return WithTestClientAsync(async client =>
        {
            // Arrange
            var request = CreatePropertyClaimRequest();

            // Act
            var postResponse = await client.PostAsJsonAsync(
                "/api/v1/claims",
                request);

            Assert.Equal(
                HttpStatusCode.Created,
                postResponse.StatusCode);

            var submittedJson = JsonDocument.Parse(
                await postResponse.Content.ReadAsStringAsync());

            var claimId = submittedJson.RootElement
                .GetProperty("id").GetGuid();

            // Retrieve claim
            var getResponse = await client.GetAsync(
                $"/api/v1/claims/{claimId}");

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

            var retrievedJson = JsonDocument.Parse(
                await getResponse.Content.ReadAsStringAsync());

            var claim = retrievedJson.RootElement;

            // Assert
            Assert.Equal(
                "property",
                claim.GetProperty("claimType").GetString());

            Assert.Equal(
                "Jane Lim",
                claim.GetProperty("claimantName").GetString());

            Assert.Equal(
                "Kuala Lumpur",
                claim.GetProperty("propertyDetails")
                    .GetProperty("propertyAddress").GetString());

            Assert.Equal(
                JsonValueKind.Null,
                claim.GetProperty("motorDetails").ValueKind);
        });
    }

    [Fact]
    public Task SubmitInvalidClaim_ReturnsBadRequest()
    {
        return WithTestClientAsync(async client =>
        {
            var request = new
            {
                claimType = "motor",
                claimantName = "",
                claimantEmail = "invalid-email",
                claimantPhone = "0123456789",

                policyDetails = new
                {
                    policyNumber = "POL-2026-003",
                    policyholderName = "John Tan",
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
                HttpStatusCode.BadRequest,
                response.StatusCode);
        });
    }

    [Fact]
    public Task SubmitMotorClaim_WithPropertyDetails_ReturnsBadRequest()
    {
        return WithTestClientAsync(async client =>
        {
            var request = new
            {
                claimType = "motor",
                claimantName = "John Tan",
                claimantEmail = "john@example.com",
                claimantPhone = "0123456789",

                policyDetails = new
                {
                    policyNumber = "POL-2026-004",
                    policyholderName = "John Tan",
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
                },

                propertyDetails = new
                {
                    propertyAddress = "Kuala Lumpur",
                    propertyType = "Residential",
                    damageDescription = "Water damage."
                }
            };

            var response = await client.PostAsJsonAsync(
                "/api/v1/claims",
                request);

            Assert.Equal(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        });
    }

    [Fact]
    public Task RetrieveNonexistentClaim_ReturnsNotFound()
    {
        return WithTestClientAsync(async client =>
        {
            var unknownId = Guid.NewGuid();

            var response = await client.GetAsync(
                $"/api/v1/claims/{unknownId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode);
        });
    }

    // Creates a separate test database for each test.
    private static async Task WithTestClientAsync(
        Func<HttpClient, Task> test)
    {
        using var factory = new ClaimsApiFactory();

        using var client = factory.CreateClient();

        try
        {
            await factory.InitializeDatabaseAsync();

            await test(client);
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

            claimantName = "John Tan",
            claimantEmail = "john@example.com",
            claimantPhone = "0123456789",

            policyDetails = new
            {
                policyNumber = "POL-2026-001",
                policyholderName = "John Tan",
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

    private static object CreatePropertyClaimRequest()
    {
        return new
        {
            claimType = "property",

            claimantName = "Jane Lim",
            claimantEmail = "jane@example.com",
            claimantPhone = "0123456789",

            policyDetails = new
            {
                policyNumber = "POL-2026-002",
                policyholderName = "Jane Lim",
                coverageStartDate = "2026-01-01",
                coverageEndDate = "2026-12-31",
                insuredAmount = 300000m,
                currencyCode = "MYR"
            },

            incidentDate = "2026-11-19",
            incidentLocation = "Kuala Lumpur",
            incidentDescription = "Water damage to a residential property.",

            propertyDetails = new
            {
                propertyAddress = "Kuala Lumpur",
                propertyType = "Residential",
                damageDescription = "Water damage to the living room."
            }
        };
    }
}

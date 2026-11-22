using System.Net;
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
            $"/api/v1/claims/{Guid.NewGuid()}"
        );

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode
        );
    }

    [Fact]
    public async Task GetClaim_AsClaimant_ReturnsForbidden()
    {
        using var factory = new ClaimsApiFactory();

        await factory.InitializeDatabaseAsync();

        using var client = factory.CreateAuthenticatedClient(
    ClaimsApiFactory.TestClaimantUserId,
    RoleNames.Claimant
);

        var response = await client.GetAsync(
            $"/api/v1/claims/{Guid.NewGuid()}"
        );

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );
    }
}
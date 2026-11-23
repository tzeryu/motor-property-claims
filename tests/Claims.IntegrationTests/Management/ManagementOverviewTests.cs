using System.Net;
using System.Text.Json;

using Claims.Application.Common.Authorization;
using Claims.IntegrationTests.Infrastructure;

using Xunit;

namespace Claims.IntegrationTests.Management;

public class ManagementOverviewTests
{
    [Fact]
    public async Task Manager_CanRetrieveOverview()
    {
        using var factory = new ClaimsApiFactory();

        try
        {
            await factory.InitializeDatabaseAsync();

            using var client = factory.CreateAuthenticatedClient(
                Guid.NewGuid(),
                RoleNames.Manager
            );

            var response = await client.GetAsync(
                "/api/v1/management/overview"
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"Expected 200 OK but received {(int)response.StatusCode} " +
                $"({response.StatusCode}). Response: {responseBody}"
            );

            using var json = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync()
            );

            Assert.Equal(
                0,
                json.RootElement
                    .GetProperty("totalOutstandingClaims")
                    .GetInt32()
            );
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task ClaimsOfficer_CannotRetrieveManagementOverview()
    {
        using var factory = new ClaimsApiFactory();

        try
        {
            await factory.InitializeDatabaseAsync();

            using var client = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOfficerUserId,
                RoleNames.ClaimsOfficer
            );


            var response = await client.GetAsync(
                "/api/v1/management/overview"
            );

            Assert.Equal(
                HttpStatusCode.Forbidden,
                response.StatusCode
            );
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }
}
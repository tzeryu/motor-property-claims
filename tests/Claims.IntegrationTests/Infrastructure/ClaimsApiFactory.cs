
using Claims.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.IntegrationTests.Infrastructure;

public sealed class ClaimsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"MotorPropertyClaims_IntegrationTests_{Guid.NewGuid():N}";

    private string ConnectionString =>
        $@"Server=(localdb)\MSSQLLocalDB;
           Database={_databaseName};
           Integrated Security=True;
           MultipleActiveResultSets=True;
           Encrypt=True;
           TrustServerCertificate=True";

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    ConnectionString
            };

            configuration.AddInMemoryCollection(settings);
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ClaimsDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ClaimsDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
    }
}

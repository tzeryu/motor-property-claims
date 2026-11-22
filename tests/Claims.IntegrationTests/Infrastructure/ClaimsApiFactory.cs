using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

using Claims.Infrastructure.Persistence;
using Claims.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Microsoft.IdentityModel.Tokens;

namespace Claims.IntegrationTests.Infrastructure;

public sealed class ClaimsApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid TestClaimantUserId =
    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid TestOtherClaimantUserId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static readonly Guid TestOfficerUserId =
        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    private readonly string _databaseName =
        $"ClaimsIntegrationTests_{Guid.NewGuid():N}";

    private const string TestJwtIssuer =
        "ClaimsApi.IntegrationTests";

    private const string TestJwtAudience =
        "ClaimsApi.IntegrationTests.Client";

    private const string TestJwtSigningKey =
        "integration-test-key-not-for-production-123456789";

    private const string TestJwtKeyId =
        "integration-test-key";

    private string TestConnectionString =>
        new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
        {
            DataSource = "(localdb)\\MSSQLLocalDB",
            InitialCatalog = _databaseName,
            IntegratedSecurity = true,
            MultipleActiveResultSets = true,
            Encrypt = true,
            TrustServerCertificate = true
        }.ConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["IdentitySeeding:Enabled"] = "false",
                    ["DevelopmentUsers:Enabled"] = "false",
                    ["Approval:ClaimsOfficerLimit"] = "10000",
                    ["Approval:CurrencyCode"] = "MYR"
                }
            );
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace the application's database configuration.
            services.RemoveAll<
                IDbContextOptionsConfiguration<ClaimsDbContext>>();

            services.RemoveAll<
                DbContextOptions<ClaimsDbContext>>();

            services.RemoveAll<ClaimsDbContext>();

            services.AddDbContext<ClaimsDbContext>(options =>
            {
                options.UseSqlServer(TestConnectionString);
            });

            // Override JWT validation specifically for integration tests.
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    var signingKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(TestJwtSigningKey))
                    {
                        KeyId = TestJwtKeyId
                    };

                    options.MapInboundClaims = false;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = TestJwtIssuer,

                            ValidateAudience = true,
                            ValidAudience = TestJwtAudience,

                            ValidateLifetime = true,
                            RequireExpirationTime = true,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = signingKey,

                            RequireSignedTokens = true,
                            TryAllIssuerSigningKeys = true,

                            NameClaimType = "name",
                            RoleClaimType = "role",

                            ClockSkew = TimeSpan.FromSeconds(30)
                        };
                });
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ClaimsDbContext>();

        EnsureTestDatabase(dbContext);

        // Apply all pending migrations first.
        await dbContext.Database.MigrateAsync();

        // Retrieve Identity's user manager.
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        // Create test users required by the claim ownership foreign key.
        await EnsureTestUserAsync(
            userManager,
            TestClaimantUserId,
            "claimant-test@example.com",
            "Test Claimant"
        );

        await EnsureTestUserAsync(
            userManager,
            TestOtherClaimantUserId,
            "other-claimant-test@example.com",
            "Other Test Claimant"
        );

        await EnsureTestUserAsync(
            userManager,
            TestOfficerUserId,
            "officer-test@example.com",
            "Test Officer"
        );
    }

    private static async Task EnsureTestUserAsync(
    UserManager<ApplicationUser> userManager,
    Guid userId,
    string email,
    string fullName)
    {
        var existingUser = await userManager.FindByIdAsync(
            userId.ToString()
        );

        if (existingUser is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            FullName = fullName,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description)
            );

            throw new InvalidOperationException(
                $"Failed to create test user '{email}': {errors}"
            );
        }
    }

    public async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ClaimsDbContext>();

        // Safety check: never delete the development database.
        EnsureTestDatabase(dbContext);

        await dbContext.Database.EnsureDeletedAsync();
    }

    private void EnsureTestDatabase(ClaimsDbContext dbContext)
    {
        var actualDatabase =
            dbContext.Database.GetDbConnection().Database;

        if (!string.Equals(
                actualDatabase,
                _databaseName,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Unsafe database operation. Expected '{_databaseName}' " +
                $"but connected to '{actualDatabase}'."
            );
        }
    }

    public HttpClient CreateAuthenticatedClient(params string[] roles)
    {
        return CreateAuthenticatedClient(
            TestClaimantUserId,
            roles
        );
    }

    public HttpClient CreateAuthenticatedClient(
        Guid userId,
        params string[] roles)
    {
        var client = CreateClient();

        var token = CreateAccessToken(userId, roles);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private string CreateAccessToken(Guid userId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub, userId.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                "integration@example.com"),

            new("name", "Integration Test User"),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(TestJwtSigningKey))
        {
            KeyId = TestJwtKeyId
        };

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: TestJwtIssuer,
            audience: TestJwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddSeconds(-30),
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
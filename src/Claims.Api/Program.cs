using Claims.Infrastructure.Extensions;
using Claims.Infrastructure.Persistence;
using Claims.Application.Claims.Commands.SubmitClaim;
using Claims.Application.Claims.Queries.GetClaimById;
using System.Text;
using Claims.Application.Common.Authentication;
using Claims.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Claims.Application.Claims.Queries.GetClaims;
using Claims.Application.Claims.Commands.ManageClaimReview;
using Claims.Application.Claims.Commands.ManageClaimDecision;
using Claims.Application.Claims.Interfaces;
using Claims.Application.Claims.Commands.ManageSettlement;
using Claims.Application.Claims.Queries.GetManagementOverview;


var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection(
    JwtOptions.SectionName
);

var jwtSettings = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing."
    );

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience) ||
    string.IsNullOrWhiteSpace(jwtSettings.SigningKey) ||
    Encoding.UTF8.GetByteCount(jwtSettings.SigningKey) < 32)
{
    throw new InvalidOperationException(
        "JWT configuration is invalid. Check your issuer, audience and signing key."
    );
}

builder.Services.Configure<JwtOptions>(jwtSection);

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme
)
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("JwtAuthentication");

            logger.LogError(
                context.Exception,
                "JWT authentication failed."
            );

            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SigningKey)
            ),

            NameClaimType = "name",
            RoleClaimType = "role",

            ClockSkew = TimeSpan.FromSeconds(30)
        };
});

builder.Services.AddAuthorization();



var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found."
    );

builder.Services.AddInfrastructure(connectionString);


// Application services
builder.Services.AddScoped<SubmitClaimHandler>();

// System clock
builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<GetClaimByIdHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<GetClaimsHandler>();
builder.Services.AddScoped<ClaimReviewHandler>();
builder.Services.AddScoped<SettlementHandler>();
builder.Services.AddScoped<GetManagementOverviewHandler>();

// Add services to the container.


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter(
                System.Text.Json.JsonNamingPolicy.CamelCase,
                allowIntegerValues: false));
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<DevelopmentUsersOptions>(
    builder.Configuration.GetSection("DevelopmentUsers")
);

var officerApprovalLimit =
    builder.Configuration.GetValue<decimal?>(
        "Approval:ClaimsOfficerLimit"
    ) ?? throw new InvalidOperationException(
        "Claims Officer approval limit is not configured."
    );

var authorityCurrencyCode =
    builder.Configuration["Approval:CurrencyCode"]
    ?? throw new InvalidOperationException(
        "Approval currency is not configured."
    );

if (officerApprovalLimit <= 0 ||
    authorityCurrencyCode.Length != 3)
{
    throw new InvalidOperationException(
        "Approval configuration is invalid."
    );
}

builder.Services.AddScoped<ClaimDecisionHandler>(serviceProvider =>
    new ClaimDecisionHandler(
        serviceProvider.GetRequiredService<IClaimRepository>(),
        serviceProvider.GetRequiredService<TimeProvider>(),
        officerApprovalLimit,
        authorityCurrencyCode
    )
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDevelopment", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() &&
    app.Configuration.GetValue<bool>("IdentitySeeding:Enabled"))
{
    using var scope = app.Services.CreateScope();

    var seeder = scope.ServiceProvider
        .GetRequiredService<IdentitySeeder>();

    // Create the three roles.
    await seeder.SeedRolesAsync();

    // Create demo accounts only when explicitly enabled.
    if (app.Configuration.GetValue<bool>(
        "DevelopmentUsers:Enabled"))
    {
        await seeder.SeedDevelopmentUsersAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("AngularDevelopment");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health/database", async (
    ClaimsDbContext db,
    CancellationToken cancellationToken) =>
{
    var isConnected = await db.Database.CanConnectAsync(
        cancellationToken);

    if (!isConnected)
    {
        return Results.Problem(
            title: "Database unavailable",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    return Results.Ok(new
    {
        Status = "Healthy",
        Database = "Connected"
    });
});

app.Run();

public partial class Program { }

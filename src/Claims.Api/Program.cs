using Claims.Infrastructure.Extensions;
using Claims.Infrastructure.Persistence;
using Claims.Application.Claims.Commands.SubmitClaim;
using Claims.Application.Claims.Queries.GetClaimById;

var builder = WebApplication.CreateBuilder(args);



var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

// Application services
builder.Services.AddScoped<SubmitClaimHandler>();

// System clock
builder.Services.AddSingleton<TimeProvider>(
    TimeProvider.System);
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<GetClaimByIdHandler>();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

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

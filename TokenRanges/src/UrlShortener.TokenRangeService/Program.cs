using Azure.Identity;
using UrlShortener.TokenRangeService;

var builder = WebApplication.CreateBuilder(args);

var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger("Startup");

// Configure Key Vault integration
var keyVaultUri = builder.Configuration["KeyVault:Vault"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultUri}.vault.azure.net/"),
        new DefaultAzureCredential());
}

var connectionString = builder.Configuration["Postgres:ConnectionString"];

logger.LogError("PGSQL Connection String");
logger.LogError(connectionString);

// if (string.IsNullOrEmpty(connectionString))
// {
//     connectionString = builder.Configuration["Postgres--ConnectionString"];
//     logger.LogInformation("Postgres--ConnectionString");
// }

// Add health checks with the connection string
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "db", "sql", "postgresql" });

// Add the TokenRangeManager with the connection string
builder.Services.AddSingleton(new TokenRangeManager(connectionString));

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "TokenRangeService");
app.MapHealthChecks("/health");
app.MapPost("/assign",
    async (AssignTokenRangeRequest request, TokenRangeManager manager) =>
    {
        var range = await manager.AssignRangeAsync(request.Key);

        return range;
    });

app.Run();
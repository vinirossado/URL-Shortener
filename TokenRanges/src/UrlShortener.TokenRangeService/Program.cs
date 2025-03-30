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

        logger.LogInformation("Key Vault integration enabled. The Right form is KeyVault:Vault");
} else if (builder.Configuration["KeyVaultName"] != null)
{
    // Fallback to the old Key Vault configuration
    keyVaultUri = builder.Configuration["KeyVaultName"];
    if (!string.IsNullOrWhiteSpace(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{keyVaultUri}.vault.azure.net/"),
            new DefaultAzureCredential());

            logger.LogInformation("Key Vault integration enabled. The Right form is KeyVaultName");
    }
}

// First try to get the connection string from configuration directly
var connectionString = builder.Configuration["Postgres:ConnectionString"];

// If not found, try the standard Key Vault secret name
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration["Postgres--ConnectionString"];
    logger.LogInformation("Postgres--ConnectionString");

}

logger.LogInformation("Postgres:ConnectionString");

// Log the connection string availability (but not the actual string for security)
Console.WriteLine($"PostgreSQL Connection String available: {!string.IsNullOrEmpty(connectionString)}");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("PostgreSQL connection string not found. Please ensure it's configured in the application settings or Key Vault.");
}

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
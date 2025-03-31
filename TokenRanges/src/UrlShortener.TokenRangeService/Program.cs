using Azure.Identity;
using Microsoft.Extensions.Logging.Console;
using UrlShortener.TokenRangeService;

var builder = WebApplication.CreateBuilder(args);

// Configure proper logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => 
{
    options.FormatterName = ConsoleFormatterNames.Json;
});
builder.Logging.AddAzureWebAppDiagnostics();

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();

// Configure Key Vault integration
logger.LogInformation("Configuring Key Vault integration");
var keyVaultUri = builder.Configuration["KeyVault:Vault"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    logger.LogInformation("Key Vault URI found: {KeyVaultUri}", keyVaultUri);
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultUri}.vault.azure.net/"),
        new DefaultAzureCredential());
}
else
{
    logger.LogWarning("No Key Vault URI found in configuration");
}

// Get the connection string from configuration
var connectionString = builder.Configuration["Postgres:ConnectionString"];
logger.LogInformation("PostgreSQL Connection String available: {Available}", connectionString);

// Try alternate key format if not found
if (string.IsNullOrEmpty(connectionString))
{
    logger.LogInformation("Trying alternate connection string format (Postgres--ConnectionString)");
    connectionString = builder.Configuration["Postgres--ConnectionString"];
    logger.LogInformation("Alternate PostgreSQL Connection String available: {Available}", connectionString);
}

if (string.IsNullOrEmpty(connectionString))
{
    logger.LogCritical("No PostgreSQL connection string found in configuration");
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

logger.LogInformation("Application startup complete, mapping endpoints");

app.MapGet("/", () => "TokenRangeService");
app.MapHealthChecks("/health");
app.MapPost("/assign",
    async (AssignTokenRangeRequest request, TokenRangeManager manager, ILogger<Program> endpointLogger) =>
    {
        endpointLogger.LogInformation("Received assign request for key: {Key}", request.Key);
        try
        {
            var range = await manager.AssignRangeAsync(request.Key);
            endpointLogger.LogInformation("Assigned token range {Start} to {End} for key {Key}", 
                range.Start, range.End, request.Key);
            return range;
        }
        catch (Exception ex)
        {
            endpointLogger.LogError(ex, "Error assigning token range for key {Key}", request.Key);
            throw;
        }
    });

logger.LogInformation("Application configured and starting");

app.Run();
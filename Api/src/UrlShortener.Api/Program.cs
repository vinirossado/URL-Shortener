using Api.Extensions;
using Azure.Identity;
using UrlShortener.Core.Urls.Add;
using UrlShortener.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Check for local development appsettings.local.json file first (not checked into source control)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
}

var keyVaultName = builder.Configuration["KeyVault:Vault"];

if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    Console.WriteLine($"Configuring Azure Key Vault: {keyVaultName}");
    try 
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{keyVaultName}.vault.azure.net/"),
            new DefaultAzureCredential(new DefaultAzureCredentialOptions 
            {
                ExcludeSharedTokenCacheCredential = true,
                ExcludeManagedIdentityCredential = false
            }));
            
        Console.WriteLine("Successfully connected to Azure Key Vault");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to connect to Azure Key Vault: {ex.Message}");
    }
}
else
{
    Console.WriteLine("No Key Vault name provided - using local configuration only");
}

// Check if CosmosDB connection string is available
var cosmosDbConnectionString = builder.Configuration["CosmosDb:ConnectionString"];
Console.WriteLine($"CosmosDb:ConnectionString available: {!string.IsNullOrEmpty(cosmosDbConnectionString)}");

if (string.IsNullOrEmpty(cosmosDbConnectionString) && builder.Environment.IsDevelopment())
{
    // For development, add an in-memory data store if no connection string is available
    Console.WriteLine("Using in-memory URL data store for development");
    builder.Services.AddSingleton<IUrlDataStore, InMemoryUrlDataStore>();
}
else 
{
    // Add Cosmos DB services
    try 
    {
        builder.Services.AddCosmosUrlDataStore(builder.Configuration);
        Console.WriteLine("CosmosDB URL data store registered");
    }
    catch (Exception ex)
    {
        if (builder.Environment.IsDevelopment())
        {
            // Fallback to in-memory for development
            Console.WriteLine($"Failed to configure CosmosDB: {ex.Message}");
            Console.WriteLine("Falling back to in-memory URL data store for development");
            builder.Services.AddSingleton<IUrlDataStore, InMemoryUrlDataStore>();
        }
        else
        {
            // In production, rethrow as we need the real database
            throw;
        }
    }
}

builder.Services.AddSingleton(TimeProvider.System);

// Add URL feature (token provider and short URL generator)
builder.Services.AddUrlFeature();

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "URL Shortener API");
app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapPost("/api/urls",
    async (AddUrlHandler handler,
        AddUrlRequest request,
        CancellationToken cancellationToken) =>
    {
        var requestWithUser = request with
        {
            CreatedBy = "vini@gmail.com"
        };
        var result = await handler.HandleAsync(requestWithUser, cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(result.Error);
        }

        return Results.Created($"/api/urls/{result.Value!.ShortUrl}",
            result.Value);
    });

app.Run();

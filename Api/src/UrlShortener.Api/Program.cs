using Api.Extensions;
using Azure.Identity;
using UrlShortener.Core.Urls.Add;
using UrlShortener.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
var keyVaultName = builder.Configuration["KeyVaultName"];

if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

// builder.Configuration
//     .SetBasePath(Directory.GetCurrentDirectory())
//     .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
//     .AddEnvironmentVariables()
//     .AddCommandLine(args);
    
var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger("Startup");
// if (string.IsNullOrWhiteSpace(keyVaultName))
// {
//     logger.LogError("KeyVault:ConnectionString is not set. Ensure the connection string is correctly configured.");
// }
// else
// {
//     logger.LogInformation($"Using Azure Key Vault '{keyVaultName}'");
// }

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services
    .AddUrlFeature()
    .AddCosmosUrlDataStore(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapGet("/", () => "URL Shortener API");
app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapGet("/urls", async (AddUrlHandler handler, CancellationToken cancellationToken) => { return await handler.GetAllUrlAsync(cancellationToken); });

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
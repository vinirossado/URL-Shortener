using Api.Extensions;
using Azure.Identity;
using UrlShortener.Core.Urls.Add;
using UrlShortener.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on the port specified by Azure
// This is critical for Azure App Service container deployments
string port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// Check for local development appsettings.local.json file first (not checked into source control)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
}

var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrEmpty(keyVaultName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

// // Check if CosmosDB connection string is available
// var cosmosDbConnectionString = builder.Configuration["CosmosDb:ConnectionString"];
// Console.WriteLine($"CosmosDb:ConnectionString available: {!string.IsNullOrEmpty(cosmosDbConnectionString)}");

builder.Services.AddSingleton(TimeProvider.System);

// Add URL feature (token provider and short URL generator)
builder.Services.AddUrlFeature();
builder.Services.AddCosmosUrlDataStore(builder.Configuration);

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

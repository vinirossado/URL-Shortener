using System.Diagnostics;
using Azure.Identity;
using HealthChecks.CosmosDb;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using UrlShortener.RedirectApi;
using UrlShortener.RedirectApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var keyVaultName = builder.Configuration["KeyVault:Vault"];

if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

builder.Services.AddHealthChecks()
    .AddAzureCosmosDB(optionsFactory: _ => new AzureCosmosDbHealthCheckOptions()
    {
        DatabaseId = builder.Configuration["DatabaseName"]!
    })
    .AddRedis(provider =>
            provider.GetRequiredService<IConnectionMultiplexer>(),
        failureStatus: HealthStatus.Degraded);

builder.Services.AddUrlReader(
    cosmosConnectionString: builder.Configuration["CosmosDb:ConnectionString"]!,
    databaseName: builder.Configuration["DatabaseName"]!,
    containerName: builder.Configuration["ContainerName"]!,
    redisConnectionString: builder.Configuration["Redis:ConnectionString"]!);

var applicationName = builder.Environment.ApplicationName ?? "RedirectApi";

var app = builder.Build();

app.MapHealthChecks("/healthz", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/", () => "Redirect API");

app.MapGet("r/{shortUrl}",
    async (string shortUrl, IShortenedUrlReader reader, CancellationToken cancellationToken) =>
    {
        var response = await reader.GetLongUrlAsync(shortUrl, cancellationToken);

        if (response.Found)
            ApplicationDiagnostics.RedirectExecutedCounter.Add(1);

        Activity.Current?.SetTag("Year", DateTime.Now.Year);

        return response switch
        {
            { Found: true, LongUrl: not null }
                => Results.Redirect(response.LongUrl, true),
            _ => Results.NotFound()
        };
    });

app.Run();
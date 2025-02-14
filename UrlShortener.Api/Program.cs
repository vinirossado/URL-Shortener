using Api.Extensions;
using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using UrlShortener.Core.Urls.Add;
using UrlShortener.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

var keyVaultName = builder.Configuration["KeyVault:Vault"];

if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

// Add services
builder.Services.AddHealthChecks();
builder.Services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto; });

builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services
    .AddUrlFeature()
    .AddCosmosUrlDataStore(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();
// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                key = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});
//
// app.MapPost("/api/urls",
//     async (AddUrlHandler handler, AddUrlRequest request, CancellationToken cancellationToken) =>
//     {
//         var requestWithUser = request with { CreatedBy = "vini@gmail.com" };
//         var result = await handler.HandleAsync(requestWithUser, cancellationToken);
//
//         if (!result.Succeeded)
//         {
//             return Results.BadRequest(result.Error);
//         }
//
//         return Results.Created($"/api/urls/{result.Value!.ShortUrl}", result.Value);
//     });


var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();

lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("\n🚀 Available Endpoints:");
    foreach (var endpoint in endpointDataSource.Endpoints)
    {
        if (endpoint is RouteEndpoint routeEndpoint)
        {
            Console.WriteLine($" - {routeEndpoint.RoutePattern.RawText}");
        }
    }
});

app.Run();
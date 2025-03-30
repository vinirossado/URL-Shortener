using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using UrlShortener.TokenRangeService;

var builder = WebApplication.CreateBuilder(args);

var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}


// builder.Services.AddHealthChecks()
//     .AddNpgSql(builder.Configuration["Postgres:ConnectionString"]!);

builder.Services.AddSingleton(
    new TokenRangeManager(builder.Configuration["Postgres:ConnectionString"]!));

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "TokenRangeService");
app.MapPost("/assign",
    async (AssignTokenRangeRequest request, TokenRangeManager manager) =>
    {
        var range = await manager.AssignRangeAsync(request.Key);

        return range;
    });

app.Run();
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using UrlShortener.TokenRangeService;

var builder = WebApplication.CreateBuilder(args);

var keyVaultName = builder.Configuration["KeyVault:Vault"];

if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    var uri = new Uri($"https://{keyVaultName}.vault.azure.net/");

    builder.Configuration.AddAzureKeyVault(
        uri,
        new DefaultAzureCredential());

    // Test KeyVault connection by retrieving a secret
    var secretClient = new SecretClient(uri, new DefaultAzureCredential());
    try
    {
        // var secret = secretClient.GetSecret("TestSecret");
        var secret = secretClient.GetSecret("Postgres--ConnectionString");
        Console.WriteLine($"Successfully retrieved secret: {secret.Value.Value}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to retrieve secret: {ex.Message}");
    }
}

builder.Services.AddOpenApi();
builder.Services.AddSingleton(
    new TokenRangeManager(builder.Configuration["Postgres:ConnectionString"]!));

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
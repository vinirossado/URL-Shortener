using UrlShortener.TokenRangeService;

var builder = WebApplication.CreateBuilder(args);

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

public record AssignTokenRangeRequest(string Key);

public record TokenRangeResponse(long Start, long End);
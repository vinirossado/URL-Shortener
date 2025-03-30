using UrlShortener.Core;

namespace Api;

public class TokenManager : IHostedService
{
    private readonly ITokenRangeApiClient _client;
    private readonly string _machineIdentifier;
    private readonly TokenProvider _tokenProvider;
    private readonly IEnvironmentManager _environmentManager;

    private ILogger<TokenManager> _logger;

    public TokenManager(ITokenRangeApiClient client,
        TokenProvider tokenProvider,
        IEnvironmentManager environmentManager,
        ILogger<TokenManager> logger)
    {
        _client = client;
        _tokenProvider = tokenProvider;
        _environmentManager = environmentManager;

        _machineIdentifier = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? "unknown";
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting token manager");

            _tokenProvider.ReachingRangeLimit += async (sender, args) =>
            {
                await AssignNewRangeAsync(cancellationToken);
            };

            await AssignNewRangeAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "TokenManager failed to start due to an error.");
            _environmentManager.FatalError(); // Stop the application with a fatal error
        }
    }

    private async Task AssignNewRangeAsync(CancellationToken cancellationToken)
    {
        var range = await _client.AssignRangeAsync(_machineIdentifier, cancellationToken);
        
        if (range is null)
        {
            throw new Exception("No tokens assigned");
        }

        _tokenProvider.AssignRange(range);
        _logger.LogInformation("Assigned range: {Start}-{End}", range.Start, range.End);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping token manager");
        return Task.CompletedTask;
    }
}
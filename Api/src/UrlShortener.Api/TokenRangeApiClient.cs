using UrlShortener.Core;

namespace Api;

public class TokenRangeApiClient : ITokenRangeApiClient
{
    public TokenRangeApiClient()
    {
    }

    public Task<TokenRange?> AssignTokenRangeAsync(string machineIdentifier, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
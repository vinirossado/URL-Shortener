using UrlShortener.Core;

namespace Api;

public interface ITokenRangeApiClient
{
    Task<TokenRange?> AssignTokenRangeAsync(string machineIdentifier, CancellationToken cancellationToken);

}
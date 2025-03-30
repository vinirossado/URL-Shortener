using UrlShortener.Core;

namespace Api;

public interface ITokenRangeApiClient
{
    Task<TokenRange?> AssignRangeAsync(string machineIdentifier, CancellationToken cancellationToken);

}
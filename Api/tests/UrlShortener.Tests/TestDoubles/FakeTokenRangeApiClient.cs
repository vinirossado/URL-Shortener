using Api;
using UrlShortener.Core;

namespace UrlShortener.Tests.TestDoubles;

public class FakeTokenRangeApiClient : ITokenRangeApiClient
{
    public Task<TokenRange?> AssignRangeAsync(string machineIdentifier, CancellationToken cancellationToken)
    {
       return Task.FromResult<TokenRange?>(new TokenRange(1, 10));
    }
}
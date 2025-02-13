using UrlShortener.Core.Urls;
using UrlShortener.Core.Urls.Add;

namespace UrlShortener.Api.Core.Tests.TestDoubles;

public class InMemoryUrlDataStore : Dictionary<string, ShortenedUrl>, IUrlDataStore
{
    public Task AddAsync(ShortenedUrl shortnedUrl, CancellationToken cancellationToken)
    {
        Add(shortnedUrl.ShortUrl, shortnedUrl);
        return Task.CompletedTask;
    }
}
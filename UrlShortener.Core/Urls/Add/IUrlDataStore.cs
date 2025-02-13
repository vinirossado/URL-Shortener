namespace UrlShortener.Core.Urls.Add;

public interface IUrlDataStore
{
    Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken);
}
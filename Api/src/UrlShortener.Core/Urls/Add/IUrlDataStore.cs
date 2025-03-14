namespace UrlShortener.Core.Urls.Add;

public interface IUrlDataStore
{
    Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken);
    Task<IEnumerable<ShortenedUrl>> GetAllAsync(CancellationToken cancellationToken);
}
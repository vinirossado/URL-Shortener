namespace UrlShortener.Core.Urls;

public class ShortenedUrl(Uri longUrl, string shortUrl, string createdBy, DateTimeOffset createdAt)
{
    public Uri LongUrl { get; } = longUrl;
    public string ShortUrl { get; } = shortUrl;
    public string CreatedBy { get; } = createdBy;
    public DateTimeOffset CreatedAt { get; } = createdAt;
}
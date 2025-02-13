using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;
using UrlShortener.Core.Urls;
using UrlShortener.Core.Urls.Add;

namespace UrlShortener.Infrastructure;

public class CosmosDbUrlDataStore(Container container) : IUrlDataStore
{
    public async Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken)
    {
        var document = (ShortenedUrlCosmos)shortenedUrl;
        await container.CreateItemAsync(document,
            new PartitionKey(document.PartitionKey),
            cancellationToken: cancellationToken);
    }
}

internal class ShortenedUrlCosmos(string longUrl, string shortUrl, string createdBy, DateTimeOffset createdAt)
{
    public string LongUrl { get; } = longUrl;

    [JsonPropertyName("id")] //Cosmos DB Unique Identifier
    public string ShortUrl { get; } = shortUrl;

    public DateTimeOffset CreatedAt { get; } = createdAt;
    public string CreatedBy { get; } = createdBy;
    public string PartitionKey => ShortUrl[..1]; // Cosmos DB Partition Key

    public static implicit operator ShortenedUrl(ShortenedUrlCosmos url) => new(new Uri(url.LongUrl), url.ShortUrl, url.CreatedBy, url.CreatedAt);
    public static implicit operator ShortenedUrlCosmos(ShortenedUrl url) => new(url.LongUrl.ToString(), url.ShortUrl, url.CreatedBy, url.CreatedAt);
}
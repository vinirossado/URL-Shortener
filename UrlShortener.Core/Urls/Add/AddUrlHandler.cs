namespace UrlShortener.Core.Urls.Add;

public class AddUrlHandler(ShortUrlGenerator shortUrlGenerator, IUrlDataStore urlDataStore, TimeProvider timeProvider)
{
    public async Task<Result<AddUrlResponse>> HandleAsync(AddUrlRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CreatedBy))
            return Errors.MissingCreatedBy;

        var shortenedUrl = new ShortenedUrl(request.LongUrl, shortUrlGenerator.GenerateUniqueUrl(), request.CreatedBy, timeProvider.GetUtcNow());

        await urlDataStore.AddAsync(shortenedUrl, cancellationToken);

        return new AddUrlResponse(request.LongUrl, shortUrlGenerator.GenerateUniqueUrl());
    }
}
using UrlShortener.Core;

namespace UrlShortener.Api.Core.Tests;

public class AddUrlScenarios
{
    [Fact]
    public async void Should_Return_Shortened_Url()
    {
        var tokenProvider = new TokenProvider();
        tokenProvider.AssignRange(1,5);
        var shortUrlGenerator = new ShortUrlGenerator(tokenProvider);
        var handler = new AddUrlHandler(shortUrlGenerator);
        var request = new AddUrlRequest(new Uri("https://www.google.com"));
        
        var response = await handler.HandleAsync(request, default);
        response.ShortUrl.Should().NotBeEmpty();
        response.ShortUrl.Should().Be("1");
    }
}

public record AddUrlRequest(Uri LongUrl);
public record AddUrlResponse(Uri LongUrl, string ShortUrl);

public class AddUrlHandler(ShortUrlGenerator shortUrlGenerator)
{
    public Task<AddUrlResponse> HandleAsync(AddUrlRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AddUrlResponse(request.LongUrl, "1"));
    }
}
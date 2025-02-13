using Microsoft.Extensions.Time.Testing;
using UrlShortener.Api.Core.Tests.TestDoubles;
using UrlShortener.Core;
using UrlShortener.Core.Urls.Add;

namespace UrlShortener.Api.Core.Tests.Urls;

public class AddUrlScenarios
{
    private readonly AddUrlHandler _handler;
    private readonly InMemoryUrlDataStore _urlDataStore = new();
    private readonly FakeTimeProvider _timeProvider;

    public AddUrlScenarios()
    {
        var tokenProvider = new TokenProvider();
        tokenProvider.AssignRange(1, 5);
        var shortUrlGenerator = new ShortUrlGenerator(tokenProvider);
        _timeProvider = new FakeTimeProvider();
        _handler = new AddUrlHandler(shortUrlGenerator, _urlDataStore, _timeProvider);
    }

    [Fact]
    public async Task Should_Return_Shortened_Url()
    {
        var request = CreateAddUrlRequest();

        var response = await _handler.HandleAsync(request, CancellationToken.None);

        response.Value!.ShortUrl.Should().NotBeEmpty();
        response.Value!.ShortUrl.Should().Be("1");
    }

    [Fact]
    public async Task Should_Save_Short_Url()
    {
        var request = CreateAddUrlRequest();

        var response = await _handler.HandleAsync(request, CancellationToken.None);

        _urlDataStore.Should().ContainKey(response.Value!.ShortUrl);
    }

    [Fact]
    public async Task Should_Save_Short_Url_With_Created_By_And_Created_At()
    {
        var request = CreateAddUrlRequest();

        var response = await _handler.HandleAsync(request, CancellationToken.None);

        response.Succeeded.Should().BeTrue();
        _urlDataStore.Should().ContainKey(response.Value!.ShortUrl);
        _urlDataStore[response.Value!.ShortUrl].CreatedBy.Should().Be(request.CreatedBy);
        _urlDataStore[response.Value!.ShortUrl].CreatedAt.Should().Be(_timeProvider.GetUtcNow());
    }

    [Fact]
    public async Task Should_Return_Error_If_Created_By_Is_Empty()
    {
        var request = CreateAddUrlRequest(string.Empty);

        var response = await _handler.HandleAsync(request, CancellationToken.None);

        response.Succeeded.Should().BeFalse();
        response.Error.Code.Should().Be("missing_value");
    }

    private static AddUrlRequest CreateAddUrlRequest(string createdBy = "vini@gmail.com")
    {
        return new AddUrlRequest(new Uri("https://google.com"), createdBy);
    }
}
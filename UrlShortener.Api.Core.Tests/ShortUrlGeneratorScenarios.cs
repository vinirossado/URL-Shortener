using UrlShortener.Core;

namespace UrlShortener.Api.Core.Tests;

public class ShortUrlGeneratorScenarios
{
    /*Test List
     * Unique tokens should be generated
     * Accept multiple Ranges
     * _TokenRange on TokenProvider can be null when getting token
     * Should the token range use logs instead of ints?
     */
    [Fact]
    public void Should_Return_Short_Url_For_Zero()
    {
        var tokenProvider = new TokenProvider();
        tokenProvider.AssignRange(0, 10);

        var shortUrlGenerator = new ShortUrlGenerator(tokenProvider);
        var shortUrl = shortUrlGenerator.GenerateUniqueUrl();

        shortUrl.Should().Be("0");
    }

    [Fact]
    public void Should_Return_Short_Url_For_10001()
    {
        var tokenProvider = new TokenProvider();
        tokenProvider.AssignRange(10001, 20000);

        var shortUrlGenerator = new ShortUrlGenerator(tokenProvider);

        var shortUrl = shortUrlGenerator.GenerateUniqueUrl();

        shortUrl.Should().Be("2bJ");
    }
}
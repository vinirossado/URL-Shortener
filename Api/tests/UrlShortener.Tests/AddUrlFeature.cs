using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using UrlShortener.Core.Urls.Add;

namespace UrlShortener.Tests;

public class AddUrlFeature : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public AddUrlFeature(ApiFixture fixture)
    {
        _client = fixture.CreateClient();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue(scheme: "TestScheme");
    }

    [Fact]
    public async Task Given_Long_Url_Should_Return_Short_Url()
    {
        var response = await _client.PostAsJsonAsync<AddUrlRequest>("/api/urls",
            new AddUrlRequest(new Uri("https://google.com"), ""));
    
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var addUrlResponse = await response.Content.ReadFromJsonAsync<AddUrlResponse>();
        addUrlResponse!.ShortUrl.Should().NotBeNull();
    }
}
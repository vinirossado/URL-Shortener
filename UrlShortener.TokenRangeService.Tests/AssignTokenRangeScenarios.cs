using System.Collections.Concurrent;
using System.Net.Http.Json;
using FluentAssertions;

namespace UrlShortener.TokenRangeService.Tests;

public class AssignTokenRangeScenarios(Fixture fixture) : IClassFixture<Fixture>
{
    private readonly HttpClient _client = fixture.CreateClient();

    [Fact]
    public async Task Should_Return_Range_When_Requested()
    {
        var requestResponse = await _client.PostAsJsonAsync("/assign",
            new AssignTokenRangeRequest("tests"));

        requestResponse.Should().BeSuccessful();
        var tokenRange = await requestResponse.Content
            .ReadFromJsonAsync<TokenRangeResponse>();
        tokenRange!.Start.Should().BeGreaterThan(0);
        tokenRange.End.Should().BeGreaterThan(tokenRange.Start);
    }

    [Fact]
    public async Task Should_Not_Repeat_Range_When_Requested()
    {
        var requestResponse1 = await _client.PostAsJsonAsync("/assign",
            new AssignTokenRangeRequest("tests"));
        var requestResponse2 = await _client.PostAsJsonAsync("/assign",
            new AssignTokenRangeRequest("tests"));

        requestResponse1.Should().BeSuccessful();
        requestResponse2.Should().BeSuccessful();
        var tokenRange1 = await requestResponse1.Content
            .ReadFromJsonAsync<TokenRangeResponse>();
        var tokenRange2 = await requestResponse2.Content
            .ReadFromJsonAsync<TokenRangeResponse>();
        tokenRange2!.Start.Should().BeGreaterThan(tokenRange1!.End);
    }

    [Fact]
    public async Task Should_Not_Repeat_Range_on_Multiple_Requests()
    {
        ConcurrentBag<TokenRangeResponse> ranges = [];
        await Parallel.ForEachAsync(Enumerable.Range(1, 100), async (number, cancellationToken) =>
        {
            var response = await _client
                .PostAsJsonAsync("/assign",
                    new AssignTokenRangeRequest(number.ToString()),
                    cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var range = await response.Content
                    .ReadFromJsonAsync<TokenRangeResponse>(cancellationToken: cancellationToken);
                ranges.Add(range!);
            }
        });

        ranges.Should().OnlyHaveUniqueItems(x => x.Start);
        ranges.Should().OnlyHaveUniqueItems(x => x.End);
    }
}
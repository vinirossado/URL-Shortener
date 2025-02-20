
using System.Collections.Concurrent;
using UrlShortener.Core;

namespace UrlShortener.Api.Core.Tests;

public class TokenProviderScenarios
{
    [Fact]
    public void Should_Get_The_Token_From_Start()
    {
        var provider = new TokenProvider();

        provider.AssignRange(5, 10);

        provider.GetToken().Should().Be(5);
    }

    [Fact]
    public void Should_Increment_Token_On_Get()
    {
        var provider = new TokenProvider();

        provider.AssignRange(5, 10);
        provider.GetToken();

        provider.GetToken().Should().Be(6);
    }

    [Fact]
    public void Should_Return_Null_When_Range_Not_Assigned()
    {
        var provider = new TokenProvider();

        provider.GetToken().Should().Be(0);
    }

    [Fact]
    public void Should_Not_Return_Same_Token_Twice()
    {
        var provider = new TokenProvider();
        ConcurrentBag<long> tokens = [];
        const int start = 1;
        const int end = 10000;
        provider.AssignRange(start, end);

        Parallel.ForEach(Enumerable.Range(start, end),
            _ => tokens.Add(provider.GetToken()));

        tokens.Should().OnlyHaveUniqueItems();
    }
}
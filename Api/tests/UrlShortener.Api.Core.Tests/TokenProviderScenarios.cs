
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

    [Fact]
    public void Should_use_multiple_ranges()
    {
        var provider = new TokenProvider();
        provider.AssignRange(1, 2);
        provider.AssignRange(42, 45);
        provider.GetToken(); // 1
        provider.GetToken(); // 2
        
        var token = provider.GetToken();
        
        token.Should().Be(42);
    }

    [Fact]
    public void Should_trigger_reaching_limit_event_when_range_is_At_80_percent()
    {
        var provider = new TokenProvider();
        provider.AssignRange(1, 10);

        bool eventTriggered = false;
        provider.ReachingRangeLimit += (sender, args) =>
        {
            eventTriggered = true;
            // args.Token.Should().Be(8);
            // args.RangeLimit.Should().Be(10);
        };
        
        for (var i = 0; i < 8; i++)
        {
            provider.GetToken();
        }
        
        eventTriggered.Should().BeTrue();
    }
}
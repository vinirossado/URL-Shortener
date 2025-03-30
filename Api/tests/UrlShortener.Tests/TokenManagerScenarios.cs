using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UrlShortener.Core;

namespace UrlShortener.Tests;

public class TokenManagerScenarios
{
    [Fact]
    public async Task Should_call_api_on_Start()
    {
        var tokenRangeApiClient = Substitute.For<ITokenRangeApiClient>();

        tokenRangeApiClient
            .AssignTokenRangeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TokenRange(1, 10));

        var tokenManager = new TokenManager(
            tokenRangeApiClient,
            Substitute.For<TokenProvider>(),
            Substitute.For<IEnvironmentManager>(),
            Substitute.For<ILogger<TokenManager>>());

        await tokenManager.StartAsync(CancellationToken.None);

        await tokenRangeApiClient.Received().AssignTokenRangeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_throw_exception_when_no_tokens_assigned()
    {
        
        var tokenRangeApiClient = Substitute.For<ITokenRangeApiClient>();

        tokenRangeApiClient
            .AssignTokenRangeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((TokenRange?)null);

        var tokenManager = new TokenManager(
            tokenRangeApiClient,
            Substitute.For<TokenProvider>(),
            Substitute.For<IEnvironmentManager>(),
            Substitute.For<ILogger<TokenManager>>());

        var action =()=> tokenManager.StartAsync(CancellationToken.None);
        
        await action.Should().ThrowAsync<Exception>().WithMessage("No tokens assigned.");
        
    }
}
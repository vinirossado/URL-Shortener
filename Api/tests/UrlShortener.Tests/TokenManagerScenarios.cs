using Api;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using UrlShortener.Core;

namespace UrlShortener.Tests;

public class TokenManagerScenarios
{
    [Fact]
    public async Task Should_call_api_on_Start()
    {
        var tokenRangeApiClient = Substitute.For<ITokenRangeApiClient>();

        tokenRangeApiClient
            .AssignRangeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new TokenRange(1, 10));

        var tokenManager = new TokenManager(
            tokenRangeApiClient,
            Substitute.For<TokenProvider>(),
            Substitute.For<IEnvironmentManager>(),
            Substitute.For<ILogger<TokenManager>>());

        await tokenManager.StartAsync(CancellationToken.None);

        await tokenRangeApiClient.Received().AssignRangeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_throw_exception_when_no_tokens_assigned()
    {
        var tokenRangeApiClient = Substitute.For<ITokenRangeApiClient>();
        var environmentManager = Substitute.For<IEnvironmentManager>();

        var tokenManager = new TokenManager(
            tokenRangeApiClient,
            Substitute.For<TokenProvider>(),
            environmentManager,
            Substitute.For<ILogger<TokenManager>>());

        await tokenManager.StartAsync(CancellationToken.None);
        
        environmentManager.Received().FatalError();
        
    }
}
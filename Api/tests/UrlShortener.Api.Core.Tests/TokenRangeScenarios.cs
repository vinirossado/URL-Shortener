using UrlShortener.Core;

namespace UrlShortener.Api.Core.Tests;

public class TokenRangeScenarios
{
    [Fact]
    public void When_Start_Token_Is_Greater_Than_End_Token_Then_Throws_Exception()
    {
        var act = () => new TokenRange(10, 5);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Start token should be less than or equal to end token");
    }
}
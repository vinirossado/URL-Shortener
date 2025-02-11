namespace UrlShortener.Core;

public class TokenProvider
{
    private TokenRange? _tokenRange;
    public void AssignRange(int start, int end)
    {
        _tokenRange = new TokenRange(start, end);
    }

    public void AssignRange(TokenRange tokenRange)
    {
        _tokenRange = tokenRange;
    }

    public long GetToken()
    {
        return _tokenRange.Start;
    }
}
namespace UrlShortener.Core;

public record TokenRange
{
    public TokenRange(long start, long end)
    {
        if (end < start)
            throw new ArgumentException("Start token should be less than or equal to end token");

        Start = start;
        End = end;
    }

    public long Start { get; }
    public long End { get; }
}
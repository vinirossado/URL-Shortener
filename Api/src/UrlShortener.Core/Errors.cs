namespace UrlShortener.Core;

public static class Errors
{
    public static Error MissingCreatedBy => new Error("missing_value", "Created by is required");
}
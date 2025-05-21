using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace UrlShortener.CosmosDbTriggerFunction;

public class UrlShortenerPropagation
{
    private readonly ILogger<UrlShortenerPropagation> _logger;

    public UrlShortenerPropagation(ILogger<UrlShortenerPropagation> logger)
    {
        _logger = logger;
    }

    [Function("UrlShortenerPropagation")]
    public void Run([CosmosDBTrigger(
        databaseName: "urls",
        containerName: "items",
        Connection = "CosmosDbConnection",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<MyDocument> input)
    {
        if (input != null && input.Count > 0)
        {
            _logger.LogInformation("Documents modified: {Count}", input.Count);
            
            foreach (var document in input)
            {
                _logger.LogInformation("Processing document with ID: {Id}", document.id);
                _logger.LogInformation("URL: {LongUrl} -> {ShortUrl}", document.LongUrl, document.ShortUrl);
                
                // Here you would implement your logic to handle URL changes
                // For example, you might want to:
                // 1. Update a cache
                // 2. Send a notification
                // 3. Update another database
                // 4. Trigger analytics
            }
        }
    }
}

public class MyDocument
{
    public string id { get; set; } = string.Empty;

    public string? Token { get; set; }
    
    public string? LongUrl { get; set; }
    
    public string? ShortUrl { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string? UserId { get; set; }
    
    public string? Type { get; set; } = "url";
}
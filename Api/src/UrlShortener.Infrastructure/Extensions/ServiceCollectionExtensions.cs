using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core.Urls.Add;

namespace UrlShortener.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosUrlDataStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get connection string from configuration (Key Vault)
        var connectionString = configuration["CosmosDb:ConnectionString"];
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Cosmos DB connection string is missing. Check your Key Vault configuration.");
        }
        
        services.AddSingleton<CosmosClient>(s => new CosmosClient(connectionString));
        
        services.AddSingleton<IUrlDataStore>(s =>
        {
            var cosmosClient = s.GetRequiredService<CosmosClient>();
            var databaseName = configuration["DatabaseName"] ?? throw new InvalidOperationException("DatabaseName configuration is missing");
            var containerName = configuration["ContainerName"] ?? throw new InvalidOperationException("ContainerName configuration is missing");
            
            var container = cosmosClient.GetContainer(databaseName, containerName);
            
            return new CosmosDbUrlDataStore(container);
        });

        return services;
    }
}
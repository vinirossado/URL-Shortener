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
        services.AddSingleton<CosmosClient>(s =>
            new CosmosClient(
                "AccountEndpoint=https://cosmos-db-2g45mzh7gjumo.documents.azure.com:443/;AccountKey=uoovgHuipyzwG74vteFwDjKppzTLVMX9zBHVK5mRRx45DgG28MIlLvJoYUbOV3ge6gf4W9XO6GQHACDbF8956w==;"));
        
        services.AddSingleton<IUrlDataStore>(s =>
        {
            var cosmosClient = s.GetRequiredService<CosmosClient>();

            var container = cosmosClient.GetContainer(
                configuration["DatabaseName"]!,
                configuration["ContainerName"]!);
            
            return new CosmosDbUrlDataStore(container);
        });

        return services;
    }
}
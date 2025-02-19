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
                "AccountEndpoint=https://cosmos-db-k4mcxdyfbnuxo.documents.azure.com:443/;AccountKey=jfHfLaEg2ZPcPPYlo6qqrYTnInnPaDDddCMDsRdJP9QX1n7SKQfHdhp4tTFoxkH9puvhYCAju6chACDbyFz5GA==;"));
        
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
using Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core.Urls;
using UrlShortener.Core.Urls.Add;
using UrlShortener.Tests.Extensions;


namespace UrlShortener.Tests;

public class ApiFixture : WebApplicationFactory<IApiAssemblyMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(
            services =>
            {
                services.Remove<IUrlDataStore>();
                services.AddSingleton<IUrlDataStore>(new InMemoryUrlDataStore());
            });
        
        base.ConfigureWebHost(builder);
    }
}

public class InMemoryUrlDataStore : Dictionary<string, ShortenedUrl>, IUrlDataStore
{
    public Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken)
    {
        Add(shortenedUrl.ShortUrl, shortenedUrl);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ShortenedUrl>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
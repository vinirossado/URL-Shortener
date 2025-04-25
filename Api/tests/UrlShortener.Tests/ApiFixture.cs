using Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UrlShortener.Core.Urls.Add;
using UrlShortener.Tests.Extensions;
using UrlShortener.Tests.TestDoubles;


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

                services.Remove<ITokenRangeApiClient>();
                services.AddSingleton<ITokenRangeApiClient, FakeTokenRangeApiClient>();

                services.AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>
                        ("TestScheme", options => { });

                services.AddAuthorization(opt =>
                {
                    opt.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    opt.FallbackPolicy = null;
                });
            });
        
        base.ConfigureWebHost(builder);
    }
}
using Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UrlShortener.Tests;

public class ApiFixture : WebApplicationFactory<IApiAssemblyMarker>
{
}
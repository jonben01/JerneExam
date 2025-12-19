using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public static class TestRoot
{
    private static readonly Lazy<ServiceProvider> _provider = new(() =>
    {
        var services = new ServiceCollection();
        new Startup().ConfigureServices(services);
        return services.BuildServiceProvider();
    });
    
    public static ServiceProvider Provider => _provider.Value;
}
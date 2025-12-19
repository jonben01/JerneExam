using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.DatabaseUtil;

namespace Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddOptions();
        services.AddDataProtection();
        
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbConnectionString"] = "blank",
                ["Frontend_Origin"] = "http://localhost",
                
                ["Jwt:Key"] = "TESTKEY17240982738497210398471289034728930471209847",
                ["Jwt:Issuer"] = "JonasJerneIF",
                ["Jwt:Audience"] = "JonasJerneIF-Users",
                ["Jwt:ExpiryInMinutes"] = "60",
                
                ["Smtp:Host"] = "smtp.test.local",
                ["Smtp:Port"] = "587",
                ["Smtp:Username"] = "test",
                ["Smtp:Password"] = "password",
                ["Smtp:EnableSsl"] = "true",
                ["Smtp:FromAddress"] = "test@test.local",
                ["Smtp:FromDisplayName"] = "test@test.test"
                
            })
            .Build();
        
        services.AddSingleton(config);
        
        Program.ConfigureServices(services, config);

        services.AddSingleton<PostgresContainerManager>();
        
        services.RemoveAll<DbContextOptions<MyDbContext>>();
        services.RemoveAll<MyDbContext>();

        services.AddScoped<MyDbContext>(sp =>
        {
            var pg = sp.GetRequiredService<PostgresContainerManager>();
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseNpgsql(pg.ConnectionString)
                .Options;

            var context = new MyDbContext(options);
            return context;
        });
        
        services.AddSingleton<DatabaseInitializer>();
        services.RemoveAll<ISeeder>();
        
        services.AddScoped<ISeeder, Seeder>();
    }
}
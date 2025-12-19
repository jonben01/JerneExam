using DataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.DatabaseUtil;

public class DatabaseInitializer
{
    private static int _done;

    public DatabaseInitializer(IServiceProvider sp)
    {
        if (Interlocked.Exchange(ref _done, 1) == 1)
        {
            return;
        }

        using var scope = sp.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        context.Database.EnsureCreated();
    }
}
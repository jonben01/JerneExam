using Testcontainers.PostgreSql;

namespace Tests.DatabaseUtil;

public class PostgresContainerManager : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;
    private int _started;

    public PostgresContainerManager()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString
    {
        get
        {
            EnsureStarted();
            return _container.GetConnectionString();
        }
    }

    private void EnsureStarted()
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
        {
            return;
        }
        _container.StartAsync().GetAwaiter().GetResult();
    }
    
    public async ValueTask DisposeAsync()
    {
        if (Volatile.Read(ref _started) == 0)
        {
            return;
        }
        await _container.DisposeAsync();
    }
}
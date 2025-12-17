using Microsoft.EntityFrameworkCore;

namespace Api.Services.Util;

public static class TransactionLocks
{
    public static (int k1, int k2) UserKey(Guid userId)
    {
        var b = userId.ToByteArray();
        return (BitConverter.ToInt32(b, 0), BitConverter.ToInt32(b, 8));
    }

    public static Task AcquireUserTransactionLockAsync(DbContext context,Guid userId)
    {
        var (k1, k2) = UserKey(userId);
        return context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({k1}, {k2});");
    }

    //TODO refactor use cancellation tokens everywhere
    public static async Task<bool> TryAcquireUserTransactionLockAsync(DbContext context, Guid userId)
    {
        var (k1, k2) = UserKey(userId);
        
        return await context.Database.SqlQuery<bool>($"SELECT pg_try_advisory_xact_lock({k1}, {k2});")
            .SingleAsync();
    }
    
    //overloaded for cancellation token
    public static async Task<bool> TryAcquireUserTransactionLockAsync(DbContext context, Guid userId, CancellationToken ct)
    {
        var (k1, k2) = UserKey(userId);
        
        return await context.Database.SqlQuery<bool>($"SELECT pg_try_advisory_xact_lock({k1}, {k2});")
            .SingleAsync(cancellationToken: ct);
    }
}
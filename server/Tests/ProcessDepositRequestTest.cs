using Api.Services.Interfaces;
using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DatabaseUtil;

namespace Tests;

public class ProcessDepositRequestTest
{
    //dont really save much by repeating this here
    private static async Task<IServiceScope> ArrangeDbAndNewScopeAsync(bool seed = true)
    {
        var scope = TestRoot.Provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureUsersSeededOnceAsync();
        await seeder.Clear();

        if (seed)
        {
            await seeder.Seed();
        }
        
        return scope;
    }

    // I could have potentially checked that it correctly updates a players balance, but the fact that it will find
    // the transaction in the db already means it would update balance. (total Deposits - total Purchases = balance)
    [Fact]
    public async Task ApproveDepositAsync_HappyPath_UpdatesStatusAndProcessedFields()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);
        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var pending = await seeder.SeedPendingDepositAsync(SeedUsers.ActivePoorId);
        
        Assert.Equal(TransactionStatus.Pending, pending.Status);
        
        await transactionService.ApproveDepositAsync(pending.Id, SeedUsers.AdminId);
        
        var updated = await db.Transactions
            .AsNoTracking()
            .SingleAsync(t => t.Id == pending.Id);
        
        Assert.Equal(TransactionType.Deposit, updated.TransactionType);
        Assert.Equal(TransactionStatus.Approved, updated.Status);

        Assert.Equal(SeedUsers.AdminId, updated.ProcessedBy);
        Assert.NotNull(updated.ProcessedAt);
        Assert.NotNull(updated.UpdatedAt);
        Assert.False(updated.IsDeleted);
    }
    
    [Fact]
    public async Task RejectDepositAsync_HappyPath_UpdatesStatusAndProcessedFields()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var pending = await seeder.SeedPendingDepositAsync(SeedUsers.ActivePoorId);
        
        Assert.Equal(TransactionStatus.Pending, pending.Status);

        await transactionService.RejectDepositAsync(pending.Id, SeedUsers.AdminId);

        var updated = await db.Transactions.AsNoTracking().SingleAsync(t => t.Id == pending.Id);

        Assert.Equal(TransactionType.Deposit, updated.TransactionType);
        Assert.Equal(TransactionStatus.Rejected, updated.Status);

        Assert.Equal(SeedUsers.AdminId, updated.ProcessedBy);
        Assert.NotNull(updated.ProcessedAt);
        Assert.NotNull(updated.UpdatedAt);
        Assert.False(updated.IsDeleted);
    }
    
    [Fact]
    public async Task ApproveDepositAsync_NotFound_Throws()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var id = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            transactionService.ApproveDepositAsync(id, SeedUsers.AdminId));

        Assert.Equal($"Transaction with id {id} not found", ex.Message);
    }
    
    [Fact]
    public async Task ApproveDepositAsync_WrongTransactionType_Throws()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
        
        //Approved = Purchase by domain rules, and validation happens in Type then Status order, so it should be fine
        var purchase = await seeder.SeedApprovedPurchaseAsync(SeedUsers.ActivePoorId);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            transactionService.ApproveDepositAsync(purchase.Id, SeedUsers.AdminId));

        Assert.Equal("Only deposits can be processed", ex.Message);
    }
    
    [Fact]
    public async Task ApproveDepositAsync_ApproveTwice_SecondTimeThrows()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var pending = await seeder.SeedPendingDepositAsync(SeedUsers.ActivePoorId);

        await transactionService.ApproveDepositAsync(pending.Id, SeedUsers.AdminId);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            transactionService.ApproveDepositAsync(pending.Id, SeedUsers.AdminId));

        Assert.Equal("Only pending deposits can be processed", ex.Message);
    }
    
}
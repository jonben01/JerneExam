using Api.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Tests.DatabaseUtil;

namespace Tests;

public class GetPendingDepositsTest
{
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
    
    [Fact]
    public async Task GetPendingDepositsListAsync_HappyPath_FiltersToPendingDepositsOnly()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var d1 = await seeder.SeedPendingDepositAsync(SeedUsers.ActivePoorId, amountDkk: 100, mobilePayReference: "111");
        var d2 = await seeder.SeedPendingDepositAsync(SeedUsers.ActiveRichId, amountDkk: 200, mobilePayReference: "222");

        // shouldnt appear in deposit list
        await seeder.SeedApprovedPurchaseAsync(SeedUsers.ActivePoorId);

        var result = await transactionService.GetPendingDepositsListAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(d1.Id, result.Select(x => x.TransactionId));
        Assert.Contains(d2.Id, result.Select(x => x.TransactionId));
    }

    
    [Fact]
    public async Task GetPendingDepositsListAsync_HappyPath_OrdersByCreatedAtAscending()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var t1 = DateTime.UtcNow.AddMinutes(-10);
        var t2 = DateTime.UtcNow.AddMinutes(-5);

        var d1 = await seeder.SeedPendingDepositAsync(SeedUsers.ActivePoorId, mobilePayReference: "111", createdAtUtc: t1);
        var d2 = await seeder.SeedPendingDepositAsync(SeedUsers.ActiveRichId, mobilePayReference: "222", createdAtUtc: t2);

        var result = await transactionService.GetPendingDepositsListAsync();

        Assert.Equal(new[] { d1.Id, d2.Id }, result.Select(x => x.TransactionId).ToArray());
    }
    
    [Fact]
    public async Task GetPendingDepositsListAsync_MapsFieldsCorrectly()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var createdAt = DateTime.UtcNow.AddMinutes(-3);

        var d1 = await seeder.SeedPendingDepositAsync(
            playerId: SeedUsers.ActivePoorId,
            amountDkk: 111,
            mobilePayReference: "111",
            createdAtUtc: createdAt);

        var result = await transactionService.GetPendingDepositsListAsync();

        Assert.Single(result);

        var dto = result[0];
        Assert.Equal(d1.Id, dto.TransactionId);
        Assert.Equal(SeedUsers.ActivePoorId, dto.PlayerId);
        Assert.Equal("Active Poor", dto.PlayerFullName);
        Assert.Equal("111", dto.MobilePayReference);
        Assert.Equal(111, dto.AmountDkk);
        Assert.True(
            (dto.CreatedAt - createdAt).Duration() < TimeSpan.FromMilliseconds(1),
            $"CreatedAt mismatch. Expected ~{createdAt:o}, Actual {dto.CreatedAt:o}");
    }
    
    [Fact]
    public async Task GetPendingDepositsListAsync_NoPendingDeposits_ReturnsEmptyList()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        await seeder.SeedApprovedPurchaseAsync(SeedUsers.ActivePoorId);

        var toReject = await seeder.SeedPendingDepositAsync(
            playerId: SeedUsers.ActiveRichId,
            amountDkk: 100,
            mobilePayReference: "444");

        await transactionService.RejectDepositAsync(toReject.Id, SeedUsers.AdminId);

        var result = await transactionService.GetPendingDepositsListAsync();

        Assert.Empty(result);
    }
}
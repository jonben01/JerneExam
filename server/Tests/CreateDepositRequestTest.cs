using Api.DTOs.Requests.TransactionRequests;
using Api.Services.Interfaces;
using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DatabaseUtil;

namespace Tests;

public class CreateDepositRequestTest
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
    public async Task CreateDepositRequestAsync_HappyPath_CreatesPendingDeposit()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var request = new CreateDepositRequest
        {
            AmountDkk = 250,
            MobilePayReference = "12312431"
        };
        
        await transactionService.CreateDepositRequestAsync(SeedUsers.ActivePoorId, request);
        
        var transaction = await db.Transactions.AsNoTracking().SingleAsync(t =>
            t.PlayerId == SeedUsers.ActivePoorId &&
            t.TransactionType == TransactionType.Deposit &&
            t.MobilePayReference == "12312431");
        
        
        Assert.Equal(250, transaction.AmountDkk);
        Assert.Equal(TransactionStatus.Pending, transaction.Status);

        Assert.Null(transaction.BoardId);
        Assert.Null(transaction.ProcessedBy);
        Assert.Null(transaction.ProcessedAt);
        Assert.False(transaction.IsDeleted);
        Assert.NotEqual(default, transaction.CreatedAt);
    }

    [Fact]
    public async Task CreateDepositRequestAsync_AmountIsZero_Throws()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var request = new CreateDepositRequest
        {
            AmountDkk = 0,
            MobilePayReference = "12312431"
        };
        
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            transactionService.CreateDepositRequestAsync(SeedUsers.ActiveRichId, request));
    }
    
    [Fact]
    public async Task CreateDepositRequestAsync_AmountIsNegative_Throws()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var request = new CreateDepositRequest
        {
            AmountDkk = -100,
            MobilePayReference = "12312431"
        };
        
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            transactionService.CreateDepositRequestAsync(SeedUsers.ActiveRichId, request));
    }

    [Fact]
    public async Task CreateDepositRequestAsync_MobilePayReferenceIsNull_Throws()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();

        var request = new CreateDepositRequest
        {
            AmountDkk = 250,
            MobilePayReference = null!
        };
        
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            transactionService.CreateDepositRequestAsync(SeedUsers.ActiveRichId, request));

        Assert.Equal("MobilePay transaction number is required", ex.Message);
    }
    
    [Fact]
    public async Task CreateDepositRequestAsync_RepeatMobilePayReference_IsIdempotent()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
        
        const string refNo = "999888777";
        
        var request = new CreateDepositRequest
        {
            AmountDkk = 150,
            MobilePayReference = refNo
        };
        //1st
        await transactionService.CreateDepositRequestAsync(SeedUsers.ActiveRichId, request);
        //2nd
        await transactionService.CreateDepositRequestAsync(SeedUsers.ActiveRichId, request);
        
        var count = await db.Transactions.AsNoTracking()
            .CountAsync(t => t.MobilePayReference == refNo);

        Assert.Equal(1, count);
    }
    
    
    
}

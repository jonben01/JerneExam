using System.ComponentModel.DataAnnotations;
using Api.DTOs.Requests.BoardRequests;
using Api.Services.Interfaces;
using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DatabaseUtil;

namespace Tests;

public class PurchaseBoardTest
{
    [Fact]
    public async Task PurchaseBoardAsync_HappyPath_CreatesBoard_Transaction_ReturnsNewBalance()
    {
        using var scope = TestRoot.Provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureUsersSeededOnceAsync();
        await seeder.Clear();
        await seeder.Seed();

        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();

        var game = await seeder.SeedActiveGameAsync();

        var request = new PurchaseBoardRequest()
        {
            GameId = game.Id,
            Numbers = [1, 2, 3, 16, 14],
        };

        var (dto, newBalance) = await boardService.PurchaseBoardAsync(request, SeedUsers.ActiveRichId);

        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal(game.Id, dto.GameId);
        Assert.Equal(SeedUsers.ActiveRichId, dto.PlayerId);
        Assert.Equal(5, dto.NumberCount);
        //TODO if prices could ever hypothetically change, remove this or find an accurate pricing table somehow.
        Assert.Equal(20, dto.PriceDkk);

        Assert.True(await db.Boards.AsNoTracking().AnyAsync(b => b.Id == dto.Id));

        var numbersFromDb = await db.BoardNumbers
            .AsNoTracking()
            .Where(bn => bn.BoardId == dto.Id)
            .OrderBy(bn => bn.Number)
            .Select(bn => bn.Number)
            .ToListAsync();

        Assert.Equal(request.Numbers.OrderBy(x => x).ToList(), numbersFromDb);

        var purchaseTransaction = await db.Transactions.AsNoTracking().SingleAsync(t =>
            t.BoardId == dto.Id &&
            t.PlayerId == SeedUsers.ActiveRichId &&
            t.TransactionType == TransactionType.Purchase);

        Assert.Equal(TransactionStatus.Approved, purchaseTransaction.Status);
        Assert.Equal(dto.PriceDkk, purchaseTransaction.AmountDkk);

        Assert.Equal(500 - dto.PriceDkk, newBalance);
    }

    [Fact]
    public async Task PurchaseBoardAsync_InactivePlayer_Throws()
    {
        using var scope = TestRoot.Provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureUsersSeededOnceAsync();
        await seeder.Clear();
        await seeder.Seed();

        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();

        var game = await seeder.SeedActiveGameAsync();

        var request = new PurchaseBoardRequest()
        {
            GameId = game.Id,
            Numbers = [4, 2, 5, 16, 14],
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            boardService.PurchaseBoardAsync(request, SeedUsers.InactiveId));

        Assert.Equal("Player must be active to purchase boards", ex.Message);
    }

    [Fact]
    public async Task PurchaseBoardAsync_InsufficientBalance_Throws()
    {
        using var scope = TestRoot.Provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureUsersSeededOnceAsync();
        await seeder.Clear();
        await seeder.Seed();

        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();

        var game = await seeder.SeedActiveGameAsync();

        var request = new PurchaseBoardRequest()
        {
            GameId = game.Id,
            Numbers = [4, 2, 5, 16, 14],
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            boardService.PurchaseBoardAsync(request, SeedUsers.ActivePoorId));

        Assert.Equal("Player balance too low", ex.Message);
    }


    //testing that safeguards against manipulated requests work
    [Fact]
    public async Task PurchaseBoardAsync_RepeatedNumbers_Throws()
    {
        using var scope = TestRoot.Provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureUsersSeededOnceAsync();
        await seeder.Clear();
        await seeder.Seed();

        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();
        var game = await seeder.SeedActiveGameAsync();

        var request = new PurchaseBoardRequest
        {
            GameId = game.Id,
            Numbers = [1, 2, 2, 14, 16]
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            boardService.PurchaseBoardAsync(request, SeedUsers.ActiveRichId));

        Assert.Equal("All numbers must be unique", ex.Message);
    }

    [Fact]
    public async Task PurchaseBoardAsync_WrongGameId_Throws()
    {
        using var scope = TestRoot.Provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureUsersSeededOnceAsync();
        await seeder.Clear();
        await seeder.Seed();

        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();

        var wrongGameId = Guid.NewGuid();

        var request = new PurchaseBoardRequest
        {
            GameId = wrongGameId,
            Numbers = [1, 2, 5, 14, 16]
        };

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            boardService.PurchaseBoardAsync(request, SeedUsers.ActiveRichId));

        Assert.Equal($"Game with id {wrongGameId} doesn't exist", ex.Message);
    }

    [Fact]
    public async Task PurchaseBoardAsync_DeadlinePassed_Throws()
    {
        using var scope = TestRoot.Provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureUsersSeededOnceAsync();
        await seeder.Clear();
        await seeder.Seed();

        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();
        
        //should also throw on equal time, but good luck with timing that in a test.
        var game = await seeder.SeedActiveGameAsync(DateTime.UtcNow.AddSeconds(-1));

        var request = new PurchaseBoardRequest
        {
            GameId = game.Id,
            Numbers = [1, 2, 6, 14, 16]
        };
        
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            boardService.PurchaseBoardAsync(request, SeedUsers.ActiveRichId));

        Assert.Equal("Guess deadline has passed for this game", ex.Message);
    }
    
}
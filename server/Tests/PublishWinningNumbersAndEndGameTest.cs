using Api.DTOs.Requests.BoardRequests;
using Api.DTOs.Requests.GameRequests;
using Api.Services.Interfaces;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DatabaseUtil;

namespace Tests;

public class PublishWinningNumbersAndEndGameTest
{
    private static async Task<IServiceScope> ArrangeDbAndNewScopeAsync(bool seed = true)
    {
        var scope = TestRoot.Provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureUsersSeededOnceAsync();
        await seeder.Clear();

        if (seed)
            await seeder.Seed();

        return scope;
    }
    
     [Fact]
    public async Task PublishWinningNumbersAndEndGameAsync_HappyPath_PublishesAndEndsGame_ActivatesNextGame()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        
        const int year = 2025;
        const int week = 10;

        var (current, next) = await seeder.SeedPublishableGameWithNextWeekGameAsync(
            year: year,
            week: week,
            currentGuessDeadlineUtc: DateTime.UtcNow.AddSeconds(-1), // deadline passed
            currentIsActive: true,
            currentNumbersPublishedAt: null,
            nextIsActive: false,
            nextNumbersPublishedAt: null);

        var req = new PublishWinningNumbersRequest
        {
            GameId = current.Id,
            WinningNumber1 = 1,
            WinningNumber2 = 2,
            WinningNumber3 = 3
        };

        var dto = await gameService.PublishWinningNumbersAndEndGameAsync(req);
        
        Assert.Equal(current.Id, dto.Id);
        Assert.False(dto.IsActive);
        Assert.NotNull(dto.NumbersPublishedAt);
        Assert.Equal(1, dto.WinningNumber1);
        Assert.Equal(2, dto.WinningNumber2);
        Assert.Equal(3, dto.WinningNumber3);

        
        var currentFromDb = await db.Games.AsNoTracking().SingleAsync(g => g.Id == current.Id);
        
        Assert.False(currentFromDb.IsActive);
        Assert.NotNull(currentFromDb.NumbersPublishedAt);
        Assert.Equal(1, currentFromDb.WinningNumber1);
        Assert.Equal(2, currentFromDb.WinningNumber2);
        Assert.Equal(3, currentFromDb.WinningNumber3);
        Assert.NotNull(currentFromDb.UpdatedAt);

        
        var nextFromDb = await db.Games.AsNoTracking().SingleAsync(g => g.Id == next.Id);
        
        Assert.True(nextFromDb.IsActive);
        Assert.NotNull(nextFromDb.UpdatedAt);
        Assert.Null(nextFromDb.NumbersPublishedAt);
    }
    
    [Fact]
    public async Task PublishWinningNumbersAndEndGameAsync_HappyPath_MarksWinningBoards()
    {
        //Seed needed to add balance to rich user
        using var scope = await ArrangeDbAndNewScopeAsync(seed: true);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        const int year = 2025;
        const int week = 11;

        //seed with deadline in the future to allow purchase.
        var (current, _) = await seeder.SeedPublishableGameWithNextWeekGameAsync(
            year: year,
            week: week,
            currentGuessDeadlineUtc: DateTime.UtcNow.AddHours(1),
            currentIsActive: true,
            currentNumbersPublishedAt: null,
            nextIsActive: false,
            nextNumbersPublishedAt: null);

        //buy a winner board and a loser
        var winnerReq = new PurchaseBoardRequest { GameId = current.Id, Numbers = [1, 2, 3, 4, 5] };
        var loserReq  = new PurchaseBoardRequest { GameId = current.Id, Numbers = [4, 5, 6, 7, 8] };

        var (winnerBoard, _) = await boardService.PurchaseBoardAsync(winnerReq, SeedUsers.ActiveRichId);
        var (loserBoard, _)  = await boardService.PurchaseBoardAsync(loserReq,  SeedUsers.ActiveRichId);

        //Change deadline to the past, so the game is eligible to be ended
        await db.Games
            .Where(g => g.Id == current.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(g => g.GuessDeadline, DateTime.UtcNow.AddSeconds(-1)));

        var req = new PublishWinningNumbersRequest
        {
            GameId = current.Id,
            WinningNumber1 = 1,
            WinningNumber2 = 2,
            WinningNumber3 = 3
        };

        await gameService.PublishWinningNumbersAndEndGameAsync(req);

        //check if winner won and loser lost
        var winnerFromDb = await db.Boards.AsNoTracking().SingleAsync(b => b.Id == winnerBoard.Id);
        var loserFromDb  = await db.Boards.AsNoTracking().SingleAsync(b => b.Id == loserBoard.Id);

        Assert.True(winnerFromDb.IsWinningBoard);
        Assert.False(loserFromDb.IsWinningBoard);

        Assert.NotNull(winnerFromDb.UpdatedAt);
        Assert.NotNull(loserFromDb.UpdatedAt);
    }
    
    [Fact]
    public async Task PublishWinningNumbersAndEndGameAsync_BeforeGuessDeadline_Throws()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        const int year = 2025;
        const int week = 12;

        var (current, _) = await seeder.SeedPublishableGameWithNextWeekGameAsync(
            year: year,
            week: week,
            currentGuessDeadlineUtc: DateTime.UtcNow.AddMinutes(10), // not passed
            currentIsActive: true,
            currentNumbersPublishedAt: null);

        var req = new PublishWinningNumbersRequest
        {
            GameId = current.Id,
            WinningNumber1 = 1,
            WinningNumber2 = 2,
            WinningNumber3 = 3
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            gameService.PublishWinningNumbersAndEndGameAsync(req));

        Assert.Equal("Cannot publish numbers before guess deadline", ex.Message);
        
        var fromDb = await db.Games.AsNoTracking().SingleAsync(g => g.Id == current.Id);
        Assert.True(fromDb.IsActive);
        Assert.Null(fromDb.NumbersPublishedAt);
        Assert.Null(fromDb.WinningNumber1);
        Assert.Null(fromDb.WinningNumber2);
        Assert.Null(fromDb.WinningNumber3);
    }
    
}
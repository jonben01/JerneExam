using Api.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Tests.DatabaseUtil;

namespace Tests;

public class GetPlayerBoardsTest
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

    //Helpers for figuring timezone and game week out.
    private static TimeZoneInfo DkTz()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
        }
    }

    private static (int week, int year) CurrentDkIsoWeekYear()
    {
        var dkDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DkTz()).Date;
        var week = System.Globalization.ISOWeek.GetWeekOfYear(dkDate);
        var year = System.Globalization.ISOWeek.GetYear(dkDate);
        return (week, year);
    }

    [Fact]
    public async Task GetPlayerBoardsAsync_HappyPath_ReturnsOnlyPlayersBoards()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();

        var (week, year) = CurrentDkIsoWeekYear();
        var game = await seeder.SeedGameAsync(week, year, isActive: false, numbersPublishedAt: null);

        var t1 = DateTime.UtcNow.AddMinutes(-10);
        var t2 = DateTime.UtcNow.AddMinutes(-5);
        var t3 = DateTime.UtcNow.AddMinutes(-1);

        var b1 = await seeder.SeedBoardAsync(SeedUsers.ActiveRichId, game.Id, t1);
        var b2 = await seeder.SeedBoardAsync(SeedUsers.ActiveRichId, game.Id, t2);
        var b3 = await seeder.SeedBoardAsync(SeedUsers.ActiveRichId, game.Id, t3);
        
        //add another users board, to check that we only get the right boards
        await seeder.SeedBoardAsync(SeedUsers.ActivePoorId, game.Id, DateTime.UtcNow);

        var result = await boardService.GetPlayerBoardsAsync(SeedUsers.ActiveRichId);

        Assert.Equal(3, result.Count);
    }
    
    [Fact]
    public async Task GetPlayerBoardsAsync_HappyPath_WithGameId_FiltersToThatGame()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();

        var (week, year) = CurrentDkIsoWeekYear();

        var gameA = await seeder.SeedGameAsync(week, year, isActive: false, numbersPublishedAt: null);
        var gameB = await seeder.SeedGameAsync(week, year + 1, isActive: false, numbersPublishedAt: null);

        var a1 = await seeder.SeedBoardAsync(SeedUsers.ActiveRichId, gameA.Id, DateTime.UtcNow.AddMinutes(-2));
        await seeder.SeedBoardAsync(SeedUsers.ActiveRichId, gameB.Id, DateTime.UtcNow.AddMinutes(-1));

        var result = await boardService.GetPlayerBoardsAsync(SeedUsers.ActiveRichId, gameA.Id);

        Assert.Single(result);
        Assert.Equal(a1.Id, result[0].Id);
        Assert.Equal(gameA.Id, result[0].GameId);
    }
    
    [Fact]
    public async Task GetPlayerBoardsAsync_GameIdIsEmptyGuid_DoesNotFilter()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var boardService = scope.ServiceProvider.GetRequiredService<IBoardService>();

        var (week, year) = CurrentDkIsoWeekYear();

        var gameA = await seeder.SeedGameAsync(week, year, isActive: false, numbersPublishedAt: null);
        var gameB = await seeder.SeedGameAsync(week, year + 1, isActive: false, numbersPublishedAt: null);

        await seeder.SeedBoardAsync(SeedUsers.ActiveRichId, gameA.Id, DateTime.UtcNow.AddMinutes(-2));
        await seeder.SeedBoardAsync(SeedUsers.ActiveRichId, gameB.Id, DateTime.UtcNow.AddMinutes(-1));

        var result = await boardService.GetPlayerBoardsAsync(SeedUsers.ActiveRichId, Guid.Empty);

        Assert.Equal(2, result.Count);
    }
}
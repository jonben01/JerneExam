using Api.Services.Interfaces;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class GetActiveGameTest
{
    //TODO make these test class specific and dont just copy paste them between classes :)
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
            
        } catch (TimeZoneNotFoundException)
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
    public async Task GetActiveGameAsync_AlreadyHasActiveGame_ReturnsIt()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        
        var (week, year) = CurrentDkIsoWeekYear();

        var active = await seeder.SeedGameAsync(
            weekNumber: week,
            year: year,
            isActive: true,
            numbersPublishedAt: null);
        
        var dto = await gameService.GetActiveGameAsync();

        Assert.Equal(active.Id, dto.Id);
        Assert.True(dto.IsActive);
        Assert.Null(dto.NumbersPublishedAt);
        Assert.Equal(week, dto.WeekNumber);
        Assert.Equal(year, dto.Year);
    }

    [Fact]
    public async Task GetActiveGameAsync_NoActiveGame_ActivatesCurrentWeekGame_AndReturnsIt()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        
        var (week, year) = CurrentDkIsoWeekYear();
        
        var current = await seeder.SeedGameAsync(
            weekNumber: week,
            year: year,
            isActive: false,
            numbersPublishedAt: null);
        
        var dto = await gameService.GetActiveGameAsync();
        
        Assert.Equal(current.Id, dto.Id);
        Assert.True(dto.IsActive);

        var currentFromDb = await db.Games
            .AsNoTracking()
            .SingleAsync(g => g.Id == current.Id);
        
        Assert.True(currentFromDb.IsActive);
        Assert.NotNull(currentFromDb.UpdatedAt);
    }

    [Fact]
    public async Task GetActiveGameAsync_NoGameForCurrentWeek_Throws()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);
        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
        
        var dkTz = DkTz();
        var dkDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dkTz).Date;
        var currentWeek = System.Globalization.ISOWeek.GetWeekOfYear(dkDate);
        var currentYear = System.Globalization.ISOWeek.GetYear(dkDate);
        
        var dkNextWeekDate = dkDate.AddDays(7);
        var nextWeek = System.Globalization.ISOWeek.GetWeekOfYear(dkNextWeekDate);
        var nextYear = System.Globalization.ISOWeek.GetYear(dkNextWeekDate);
        
        //Seeding next weeks game, just to ensure it wont start next weeks game
        var nextWeekGame = await seeder.SeedGameAsync(
            weekNumber: nextWeek,
            year: nextYear,
            isActive: false,
            numbersPublishedAt: null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            gameService.GetActiveGameAsync());

        Assert.Equal($"No game found for DK ISO week {currentWeek}, year {currentYear}.", ex.Message);
        
        var nextFromDb = await db.Games.AsNoTracking().SingleAsync(g => g.Id == nextWeekGame.Id);
        Assert.False(nextFromDb.IsActive);
        Assert.Null(nextFromDb.UpdatedAt);
    }

    [Fact]
    public async Task GetActiveGameAsync_TwoActiveGames_Throws()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

        var (week, year) = CurrentDkIsoWeekYear();

        await seeder.SeedGameAsync(week, year, isActive: true, numbersPublishedAt: null);
        await seeder.SeedGameAsync(week, year, isActive: true, numbersPublishedAt: null);

        //GetActiveGame uses singleOrDefault, so it throws on 2 active games.
        await Assert.ThrowsAsync<InvalidOperationException>(() => gameService.GetActiveGameAsync());
    }
}
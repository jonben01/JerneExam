using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using Api.DTOs;
using Api.DTOs.Requests.GameRequests;
using Api.DTOs.Util;
using Api.Services.Interfaces;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class GameService : IGameService
{
    
    private readonly MyDbContext _dbContext;
    private static readonly TimeZoneInfo DkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");

    public GameService(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    
    public async Task<GameDto> GetActiveGameAsync()
    {
        var active = await _dbContext.Games
            .AsNoTracking()
            .SingleOrDefaultAsync(g => g.IsActive && g.NumbersPublishedAt == null);

        if (active is not null)
        {
            return active.ToDto();
        }
        
        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        
        var nowDkDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DkTimeZone).Date;
        var currentWeek = ISOWeek.GetWeekOfYear(nowDkDate);
        var currentYear = ISOWeek.GetYear(nowDkDate);
                
        var nowUtc = DateTime.UtcNow;
        
        active = await _dbContext.Games
            .AsNoTracking()
            .SingleOrDefaultAsync(g => g.IsActive && g.NumbersPublishedAt == null);
        
        if (active is not null)
        {
            await tx.CommitAsync();
            return active.ToDto();
        }
        
        var currentGame = await _dbContext.Games
            .SingleOrDefaultAsync(g => g.Year == currentYear
                                       && g.WeekNumber == currentWeek
                                       && g.NumbersPublishedAt == null);
        
        if (currentGame is null)
            throw new InvalidOperationException($"No game found for DK ISO week {currentWeek}, year {currentYear}.");
        
        await _dbContext.Games
            .Where(g => g.IsActive && g.NumbersPublishedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsActive, false)
                .SetProperty(x => x.UpdatedAt, nowUtc));

        currentGame.IsActive = true;
        currentGame.UpdatedAt = nowUtc;

        await _dbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return currentGame.ToDto();
        
        /*
        var game = await _dbContext.Games
            .AsNoTracking()
            .SingleOrDefaultAsync(g => g.IsActive && g.NumbersPublishedAt == null);

        if (game is null)
        {
            throw new InvalidOperationException("No active game found, this shouldn't happen.");
        }
        return game.ToDto();
         */
    }

    public async Task<GameDto> GetGameByIdAsync(Guid gameId)
    {
        var game = await _dbContext.Games
            .AsNoTracking()
            .SingleOrDefaultAsync(g => g.Id == gameId);
        if (game is null)
        {
            throw new KeyNotFoundException($"No game found with id {gameId}");
        }
        return game.ToDto();
    }

    public async Task<List<GameDto>> GetGameHistoryAsync()
    {
        var games = await _dbContext.Games
            .AsNoTracking()
            .OrderByDescending(g => g.Year)
            .ThenByDescending(g => g.WeekNumber)
            .ToListAsync();
        
        return games.Select(g => g.ToDto()).ToList();
    }

    //uses guarded atomic updates - prevents ending twice
    public async Task<GameDto> PublishWinningNumbersAndEndGameAsync(PublishWinningNumbersRequest request)
    {
        ValidateWinningNumbers(request.WinningNumber1, request.WinningNumber2, request.WinningNumber3);
        
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        
        var nowUtc = DateTime.UtcNow;
        
        var rows = await _dbContext.Games
            .Where(g =>
                g.Id == request.GameId &&
                g.IsActive &&
                g.NumbersPublishedAt == null &&
               g.GuessDeadline <= nowUtc)
            .ExecuteUpdateAsync(s => s
                .SetProperty(g => g.WinningNumber1, request.WinningNumber1)
                .SetProperty(g => g.WinningNumber2, request.WinningNumber2)
                .SetProperty(g => g.WinningNumber3, request.WinningNumber3)
                .SetProperty(g => g.NumbersPublishedAt, nowUtc)
                .SetProperty(g => g.IsActive, false)
                .SetProperty(g => g.UpdatedAt, nowUtc));
        
        if (rows == 0)
        {
            var state = await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.Id == request.GameId)
                .Select(g => new
                {
                    g.IsActive,
                    g.NumbersPublishedAt,
                    g.GuessDeadline,
                })
                .FirstOrDefaultAsync();

            if (state is null)
            {
                throw new KeyNotFoundException($"No game found with id {request.GameId}");
            }
            
            if (nowUtc < state.GuessDeadline)
            {
                throw new InvalidOperationException("Cannot publish numbers before guess deadline");
            }
            
            if (state.NumbersPublishedAt is not null)
            {
                throw new InvalidOperationException($"Winning numbers have already been published");
            }
            if (!state.IsActive)
            {
                throw new InvalidOperationException("Only the active game can be ended");
            }
            
            throw new InvalidOperationException("Game already finished");
        }
        
        
        var game = await _dbContext.Games
            .AsNoTracking()
            .FirstAsync(g => g.Id == request.GameId);
        
        var winningBoardIds = await ComputeWinningBoardIdsAsync(
            game.Id, 
            request.WinningNumber1, 
            request.WinningNumber2, 
            request.WinningNumber3);

        await _dbContext.Boards.Where(b => b.GameId == game.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.IsWinningBoard, false)
                .SetProperty(b => b.UpdatedAt, nowUtc));
        
        if (winningBoardIds.Count > 0)
        {
            var winnerIds = winningBoardIds.ToList();

            await _dbContext.Boards
                .Where(b => b.GameId == game.Id && winnerIds.Contains(b.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.IsWinningBoard, true)
                    .SetProperty(b => b.UpdatedAt, nowUtc));
        }
        
        var nextGameKey = GetNextIsoWeek(game.Year, game.WeekNumber);
        
        var nextRows = await _dbContext.Games
            .Where(g => 
                g.Year == nextGameKey.year &&
                g.WeekNumber == nextGameKey.week && 
                g.NumbersPublishedAt == null &&
                !g.IsActive)
            .ExecuteUpdateAsync(s => s
                .SetProperty(g => g.IsActive, true)
                .SetProperty(g => g.UpdatedAt, nowUtc));

        if (nextRows == 0)
        {
            var nextState = await _dbContext.Games
                .AsNoTracking()
                .Where(g => g.Year == nextGameKey.year && g.WeekNumber == nextGameKey.week)
                .Select(g => new { g.IsActive, g.NumbersPublishedAt })
                .FirstOrDefaultAsync();

            if (nextState is null)
            {
                throw new InvalidOperationException($"Next game is missing, contact system administrator");
            }

            if (nextState.NumbersPublishedAt is not null)
            {
                throw new InvalidOperationException("Next game is already finished, contact system administrator");
            }

            if (nextState.IsActive)
            {
                throw new InvalidOperationException("Next game is already active, contact system administrator");
            }
            
            throw new InvalidOperationException("There was an issue with publishing winning numbers and ending the game, please contact the system administrator");
        }
        await transaction.CommitAsync();
        
        return game.ToDto();
    }

    private static void ValidateWinningNumbers(int w1, int w2, int w3)
    {
        var inRange = w1 is >= 1 and <= 16
                      && w2 is >= 1 and <= 16
                      && w3 is >= 1 and <= 16;
        if (!inRange)
        {
            throw new ValidationException("Numbers must be between 1 and 16.");
        }
        //if numbers aren't unique, then the count will be less than 3 as HashSet removes dupes
        var unique = new HashSet<int> { w1, w2, w3 };
        if (unique.Count != 3)
        {
            throw new  ValidationException("Numbers must be unique.");
        }
    }

    private async Task<List<Guid>> ComputeWinningBoardIdsAsync(Guid gameId, int w1, int w2, int w3)
    {
        return await _dbContext.Boards
            .AsNoTracking()
            .Where(b => b.GameId == gameId)
            .Where(b =>
                b.Numbers.Any(n=> n.Number == w1) 
                && b.Numbers.Any(n =>  n.Number == w2)
                && b.Numbers.Any(n => n.Number == w3))
            .Select(b => b.Id).ToListAsync();
    }

    private static (int year, int week) GetNextIsoWeek(int year, int week)
    {
        //find the monday in the week of the game
        var monday = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
        //find the monday in the week of the new game
        var nextMonday = monday.AddDays(7);
        //return the year + week, following ISO standard
        return (ISOWeek.GetYear(nextMonday), ISOWeek.GetWeekOfYear(nextMonday));
    }

    public async Task<GameAdminOverviewDto> GetGameAdminOverviewAsync(Guid gameId)
    {
        var gameDto = await  _dbContext.Games
            .AsNoTracking()
            .Where(g => g.Id == gameId)
            .Select(EntityToDtoMapper.GameToDto)
            .SingleOrDefaultAsync();
        
        if (gameDto is null)
        {
            throw new KeyNotFoundException($"No game found with id {gameId}");
        }
        
        //if numbers haven't been published return empty list, otherwise
        //if game has all winning numbers, return them. Otherwise, return empty list
        //null check pattern ensures non-null values (int? to int here)
        var winningNumbers = gameDto.NumbersPublishedAt is null 
            ? [] 
            : gameDto is { WinningNumber1: { } n1, WinningNumber2: { } n2, WinningNumber3: { } n3 }
                ? new List<int>() { n1, n2, n3 }
                : [];
        
        //using direct projection for efficiency and to avoid a potential n+1
        var boardDtos = await _dbContext.Boards
            .AsNoTracking()
            .Where(b => b.GameId == gameId)
            .OrderBy(b => b.CreatedAt)
            .Select(EntityToDtoMapper.BoardToDto)
            .ToListAsync();
        
        var winningBoardIds = gameDto.NumbersPublishedAt is null
            ? []
            : boardDtos.Where(b => b.IsWinningBoard).Select(b => b.Id).ToList();

        return new GameAdminOverviewDto
        {
            Game = gameDto,
            WinningNumbers = winningNumbers,
            TotalBoards = boardDtos.Count,
            WinningBoardIds = winningBoardIds,
            Boards = boardDtos,
            WinningBoards = winningBoardIds.Count
        };
    }
    

    //TODO probably combine with GetTotalWinnersForGame -- Can't imagine use case where you'd need them separately
    public async Task<List<BoardDto>> GetWinningBoardsForGameAsync(Guid gameId)
    {
        var hasPublishedNumbers = await _dbContext.Games
            .AsNoTracking()
            .Where(g => g.Id == gameId)
            .Select(g => g.NumbersPublishedAt != null)
            .FirstOrDefaultAsync();

        //if the game is active or the id was invalid return empty list.
        if (!hasPublishedNumbers)
        {
            return [];
        }
        
        return await _dbContext.Boards
            .AsNoTracking()
            .Where(b => b.GameId == gameId && b.IsWinningBoard)
            .OrderBy(b => b.CreatedAt)
            .Select(EntityToDtoMapper.BoardToDto)
            .ToListAsync();
    }
    
    //TODO probably combine with GetWinningBoardsForGameAsync -- Can't imagine use case where you'd need them separately
    public async Task<int> GetTotalWinningBoardCountAsync(Guid gameId)
    {
        var published = await _dbContext.Games
            .AsNoTracking()
            .Where(g => g.Id == gameId)
            .Select(g => g.NumbersPublishedAt != null)
            .FirstOrDefaultAsync();

        //if the game is active or the id was invalid return 0 winners.
        if (!published)
        {
            return 0;
        }
        
        return await _dbContext.Boards
            .AsNoTracking()
            .Where(b => b.GameId == gameId && b.IsWinningBoard)
            .CountAsync();
    }

    //TODO this will eventually get insanely bloated due to unlimited size of the call. Add Pagination/Filters(year/range)
    public async Task<List<GameStatsDto>> GetAllGameStatsAsync(CancellationToken ct = default)
    {
        var eligibleGames = await _dbContext.Games
            .AsNoTracking()
            .Where(g => g.IsActive || g.NumbersPublishedAt != null)
            .OrderByDescending(g => g.Year)
            .ThenByDescending(g => g.WeekNumber)
            .Select(g => new
            {
                g.Id,
                g.WeekNumber,
                g.Year,
                g.WinningNumber1,
                g.WinningNumber2,
                g.WinningNumber3,
                g.NumbersPublishedAt,
            })
            .ToListAsync(ct);

        if (eligibleGames.Count == 0)
        {
            return [];
        }

        var gameIds = eligibleGames.Select(g => g.Id).ToList();

        //aggregate boards for the eligible games.
        var boardStatsByGame = await _dbContext.Boards
            .AsNoTracking()
            .Where(b => gameIds.Contains(b.GameId))
            .GroupBy(b => b.GameId)
            .Select(grp => new
            {
                GameId = grp.Key,
                TotalBoards = grp.Count(),
                WinningBoards = grp.Count(b => b.IsWinningBoard),
            })
            .ToDictionaryAsync(x => x.GameId, x => x, ct);
        
        return eligibleGames.Select(g =>
        {
            boardStatsByGame.TryGetValue(g.Id, out var boardStats);

            return new GameStatsDto
            {
                GameId = g.Id,
                WeekNumber = g.WeekNumber,
                Year = g.Year,
                TotalBoards = boardStats?.TotalBoards ?? 0,
                TotalWinningBoards = boardStats?.WinningBoards ?? 0,
                WinningNumber1 = g.WinningNumber1,
                WinningNumber2 = g.WinningNumber2,
                WinningNumber3 = g.WinningNumber3,
                NumbersPublishedAt = g.NumbersPublishedAt,
                IsFinished = g.NumbersPublishedAt is not null
            };
        }).ToList();
    }

    public async Task<GameStatsDto> GetGameStatsByIdAsync(Guid gameId)
    {
        var game = await _dbContext.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game is null)
        {
            throw new KeyNotFoundException($"No game found with id {gameId}");
        }

        if (game.NumbersPublishedAt is null && !game.IsActive)
        {
            throw new KeyNotFoundException("Future games have no stats");
        }

        var boardStatsAgg = await _dbContext.Boards
            .AsNoTracking()
            .Where(b => b.GameId == gameId)
            .GroupBy(b => b.GameId)
            .Select(grp => new
            {
                TotalBoards = grp.Count(),
                TotalWinningBoards = grp.Count(b => b.IsWinningBoard)
            }).FirstOrDefaultAsync();

        return new GameStatsDto
        {
            GameId = game.Id,
            WeekNumber = game.WeekNumber,
            Year = game.Year,
            TotalBoards = boardStatsAgg?.TotalBoards ?? 0,
            TotalWinningBoards = boardStatsAgg?.TotalWinningBoards ?? 0,
            WinningNumber1 = game.WinningNumber1,
            WinningNumber2 = game.WinningNumber2,
            WinningNumber3 = game.WinningNumber3,
            NumbersPublishedAt = game.NumbersPublishedAt,
            IsFinished = game.NumbersPublishedAt is not null
        };
    }

    // checks game state and determines if board purchase should be available.
    public async Task<bool> CanAcceptNewBoardsAsync(Guid gameId)
    {
        var game = await _dbContext.Games
            .AsNoTracking()
            .Where(g => g.Id == gameId)
            .Select(g => new
            {
                g.IsActive, 
                g.NumbersPublishedAt, 
                g.GuessDeadline 
            })
            .SingleOrDefaultAsync();
        
        if (game is null) return false;
        if (!game.IsActive) return false;
        if (game.NumbersPublishedAt is not null) return false;
        
        //Deadline is stored in UTC equivalent of 17:00 DK time, DST considered. 
        var nowUtc = DateTime.UtcNow;
        
        return nowUtc < game.GuessDeadline;
    }

    //Seeds games, deadline based on 17:00 DK time (converted to utc)
    public async Task SeedGamesAsync()
    {
        //20 ish years
        const int weeksToSeed = 1040;
        
        var nowDkDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, DkTimeZone).Date;
        var currentWeek = ISOWeek.GetWeekOfYear(nowDkDate);
        var currentYear = ISOWeek.GetYear(nowDkDate);
        var currentMonday = ISOWeek.ToDateTime(currentYear, currentWeek, DayOfWeek.Monday).Date;
        
        var existingGames = await _dbContext.Games
            .AsNoTracking()
            .Select(g => new
            {
                g.Year, 
                g.WeekNumber
            })
            .ToListAsync();
        
        var existingGameKeys = existingGames
            .Select(x => (x.Year, x.WeekNumber))
            .ToHashSet();
        
        var nowUtc = DateTime.UtcNow;

        for (var i = 0; i < weeksToSeed; i++)
        {
            var mondayLocal = currentMonday.AddDays(i * 7);
            var year = ISOWeek.GetYear(mondayLocal);
            var week = ISOWeek.GetWeekOfYear(mondayLocal);
            
            if (existingGameKeys.Contains((year, week)))
            {
                continue;
            }

            var endDateLocal = mondayLocal.AddDays(7).AddTicks(-1);
            var saturdayLocal = mondayLocal.AddDays(5);
            
            var saturdayLocalTime = new DateTime(
                saturdayLocal.Year, saturdayLocal.Month, saturdayLocal.Day, 
                17, 0, 0, DateTimeKind.Unspecified);
            
            var monday = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(mondayLocal, DateTimeKind.Unspecified), DkTimeZone);
            var endDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endDateLocal, DateTimeKind.Unspecified), DkTimeZone);

            var guessDeadlineUtc = TimeZoneInfo.ConvertTimeToUtc(saturdayLocalTime, DkTimeZone);

            _dbContext.Games.Add(new DataAccess.Entities.Game
            {
                Id = Guid.NewGuid(),
                WeekNumber = week,
                Year = year,
                StartDate = monday,
                EndDate = endDate,
                GuessDeadline = guessDeadlineUtc,
                IsActive = false,

                WinningNumber1 = null,
                WinningNumber2 = null,
                WinningNumber3 = null,
                NumbersPublishedAt = null,

                CreatedAt = nowUtc,
                UpdatedAt = null,
                IsDeleted = false,
                DeletedAt = null,
            });
        }
        //ensure that we have exactly one active game
        var hasActive = await _dbContext.Games
            .AnyAsync(g => g.IsActive && g.NumbersPublishedAt == null);

        if (!hasActive)
        {
            var currentGame = await _dbContext.Games
                .FirstOrDefaultAsync(g => g.Year == currentYear && g.WeekNumber == currentWeek);

            if (currentGame is not null)
            {
                currentGame.IsActive = true;
                currentGame.UpdatedAt = nowUtc;
            }
        }
        
        await _dbContext.SaveChangesAsync();
    }
}
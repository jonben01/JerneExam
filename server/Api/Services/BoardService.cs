using System.ComponentModel.DataAnnotations;
using System.Data;
using Api.DTOs;
using Api.DTOs.Requests.BoardRequests;
using Api.DTOs.Util;
using Api.Services.Interfaces;
using Api.Services.Util;
using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class BoardService : IBoardService
{
    
    private readonly MyDbContext _context;

    private const int MinNumber = 1;
    private const int MaxNumber = 16;
    
    private static readonly IReadOnlyDictionary<int, int> PriceTable = new Dictionary<int, int>
    {
        //TODO confirm these values are right
        //5 numbers costs 20, 8 costs 160
        { 5, 20 },
        { 6, 40 },
        { 7, 80 },
        { 8, 160 }
    };

    public BoardService(MyDbContext dbContext)
    {
        _context = dbContext;
    }
    
    //TODO add method that allows for multi purchase of same board numbers
    
    //TODO return BoardPurchaseResponseDto here instead of in ctor (well, both places actually, just dont convert in ctor)
    public async Task<(BoardDto, int newBalance)> PurchaseBoardAsync(PurchaseBoardRequest request, Guid userId)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);
        
        var nowUtc = DateTime.UtcNow;
        
        var gameState = await _context.Games
            .AsNoTracking()
            .Where(g => g.Id == request.GameId)
            .Select(g => new
            {
                g.Id,
                g.IsActive,
                g.NumbersPublishedAt,
                g.GuessDeadline
            })
            .SingleOrDefaultAsync();

        if (gameState is null)
        {
            throw new KeyNotFoundException($"Game with id {request.GameId} doesn't exist");
        }

        if (!gameState.IsActive)
        {
            throw new InvalidOperationException("Game isn't active");
        }

        if (gameState.NumbersPublishedAt is not null)
        {
            throw new InvalidOperationException("Cannot purchase a board for a finished game");
        }

        if (nowUtc >= gameState.GuessDeadline)
        {
            throw new InvalidOperationException("Guess deadline has passed for this game");
        }

        await EnsurePlayerIsActiveAsync(userId);

        var numbers = request.Numbers.ToList();
        ValidateBoardNumbers(numbers);
        
        var numberCount = numbers.Count;
        var price = CalculatePrice(numberCount);
        
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        
        var acquired = await TransactionLocks.TryAcquireUserTransactionLockAsync(_context, userId);
        if (!acquired)
        {
            throw new InvalidOperationException("Another transaction is already in progress. Please try again");
        }
        
        var currentBalance = await GetPlayerBalanceInternalAsync(userId);
        if (currentBalance < price)
        {
            throw new InvalidOperationException("Player balance too low");
        }
        
        var board = new Board
        {
            Id = Guid.NewGuid(),
            GameId = request.GameId,
            PlayerId = userId,
            NumberCount = numberCount,
            PriceDkk = price,
            IsWinningBoard = false,
            SubscriptionId = null,
            CreatedAt = nowUtc,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null,
        };

        foreach (var number in numbers.OrderBy(n => n))
        {
            board.Numbers.Add(new BoardNumber
            {
                Number = number,
            });
        }
        
        _context.Boards.Add(board);

        var boardPurchaseTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PlayerId = userId,
            Player = null!,
            TransactionType = TransactionType.Purchase,
            AmountDkk = price,
            Status = TransactionStatus.Approved,
            MobilePayReference = null,
            BoardId = board.Id,
            Board = board,
            ProcessedBy = null,
            ProcessedByUser = null,
            ProcessedAt = null,
            CreatedAt = nowUtc,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null,
        };
        
        _context.Transactions.Add(boardPurchaseTransaction);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        var newBalance = currentBalance - price;
        
        var dto = await _context.Boards
            .AsNoTracking()
            .Where(b => b.Id == board.Id)
            .Select(EntityToDtoMapper.BoardToDto)
            .SingleAsync();

        return (dto, newBalance);
    }

    private async Task<int> GetPlayerBalanceInternalAsync(Guid userId)
    {
        //Deposits add to total, Purchases subtract (logic is if type = purchase then subtract, if not, add.)
        var balance = await _context.Transactions
            .AsNoTracking()
            .Where(t => 
                t.PlayerId == userId &&
                t.Status == TransactionStatus.Approved &&
                (t.TransactionType == TransactionType.Deposit || t.TransactionType == TransactionType.Purchase))
            .SumAsync(t => (int?)(
                t.TransactionType == TransactionType.Purchase
                ? -t.AmountDkk
                : t.AmountDkk)) ?? 0;
        
        //Anything below 0 doesn't matter
        return Math.Max(0, balance);
    }

    private static int CalculatePrice(int numberCount)
    {
        if (!PriceTable.TryGetValue(numberCount, out var price))
        {
            throw new ValidationException("Invalid number of fields for pricing");
        }
        
        return price;
    }

    private static void ValidateBoardNumbers(List<int> numbers)
    {
        if (numbers.Count is < 5 or > 8)
        {
            throw new ValidationException("Number of fields per board must be between 5 and 8");
        }

        if (numbers.Any(n => n < MinNumber) || numbers.Any(n => n > MaxNumber))
        {
            throw new ValidationException("Numbers must be between 1 and 16");
        }

        if (new HashSet<int>(numbers).Count != numbers.Count)
        {
            throw new ValidationException("All numbers must be unique");
        }
    }

    private async Task EnsurePlayerIsActiveAsync(Guid userId)
    {
        var playerState = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new{ u.Id, u.IsActivePlayer })
            .SingleOrDefaultAsync();

        if (playerState is null)
        {
            throw new KeyNotFoundException($"Player with id {userId} doesn't exist");
        }

        if (!playerState.IsActivePlayer)
        {
            throw new InvalidOperationException("Player must be active to purchase boards");
        }
    }

    public async Task<BoardDto> GetBoardByIdAsync(Guid boardId)
    {
        var board = await _context.Boards
            .AsNoTracking()
            .Where(b => b.Id == boardId)
            .Select(EntityToDtoMapper.BoardToDto)
            .SingleOrDefaultAsync();

        return board ?? throw new KeyNotFoundException($"Board with id {boardId} doesn't exist");
    }

    public async Task<List<BoardDto>> GetPlayerBoardsAsync(Guid userId, Guid? gameId = null)
    {
        var boards = _context.Boards
            .AsNoTracking()
            .Where(b => b.PlayerId == userId);
        
        if (gameId is not null &&  gameId != Guid.Empty) 
        {
            boards = boards.Where(b => b.GameId == gameId);
        }
        return await boards
            .OrderByDescending(b => b.CreatedAt)
            .Select(EntityToDtoMapper.BoardToDto)
            .ToListAsync();
    }

    public async Task<List<BoardDto>> GetPlayerWinningBoardsAsync(Guid userId)
    {
        return await _context.Boards
            .AsNoTracking()
            .Where(b => b.PlayerId == userId && b.IsWinningBoard)
            .OrderByDescending(b => b.CreatedAt)
            .Select(EntityToDtoMapper.BoardToDto)
            .ToListAsync();
    }

    //TODO remove this and switch to a simple balance check, then handle balance related purchasing logic in front end -
    // obviously keep balance checks in relevant methods like purchase board
    public async Task<bool> CanPlayerAffordBoardAsync(Guid userId, int numberCount)
    {
        if (!PriceTable.ContainsKey(numberCount))
        {
            throw new  ValidationException("Number of fields per board must be between 5 and 8");
        }
        
        var balance = await GetPlayerBalanceInternalAsync(userId);
        var price = CalculatePrice(numberCount);
        
        return balance >= price;
    }

    public async Task<List<BoardDto>> GetBoardsForGameAsync(Guid gameId, bool includePlayerInfo = false)
    {
        var gameExists = await _context.Games
            .AsNoTracking()
            .AnyAsync(g => g.Id == gameId);
        
        if (!gameExists) 
        {
            throw new KeyNotFoundException($"Game with id {gameId} doesn't exist");
        }
        
        var query = _context.Boards
            .AsNoTracking()
            .Where(b => b.GameId == gameId);

        if (includePlayerInfo)
        {
            query = query.Include(b => b.Player);
        }
        
        return await query
            .OrderBy(b => b.CreatedAt)
            .Select(EntityToDtoMapper.BoardToDto)
            .ToListAsync();
    }

    public async Task<List<BoardDto>> GetWinningBoardsForGameAsync(Guid gameId)
    {
        var hasPublishedNumbers = await _context.Games
            .AsNoTracking()
            .Where(g => g.Id == gameId)
            .Select(g => g.NumbersPublishedAt != null)
            .FirstOrDefaultAsync();

        if (!hasPublishedNumbers)
        {
            return [];
        }
        
        return await  _context.Boards
            .AsNoTracking()
            .Where(b => b.GameId == gameId && b.IsWinningBoard)
            .OrderBy(b => b.CreatedAt)
            .Select(EntityToDtoMapper.BoardToDto)
            .ToListAsync();
    }

    //TODO refactor to not carry adminId, seeing as there will just be 1 admin account
    public async Task DeleteBoardAsync(Guid boardId, Guid adminId)
    {
        var nowUtc = DateTime.UtcNow;
        
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var rows = await _context.Boards
            .Where(b => b.Id == boardId && !b.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.IsDeleted, true)
                .SetProperty(b => b.DeletedAt, nowUtc)
                .SetProperty(b => b.UpdatedAt, nowUtc));

        if (rows == 0)
        {
            var exists = await _context.Boards
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(b => b.Id == boardId);

            if (!exists)
            {
                throw new KeyNotFoundException($"Board with id {boardId} doesn't exist");
            }
            //board already deleted
            return;
        }
        
        var refundInfo = await _context.Boards
            .AsNoTracking()
            .Where(b => b.Id == boardId)
            .Select(b => new { b.PlayerId, b.PriceDkk })
            .SingleAsync();

        _context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            PlayerId = refundInfo.PlayerId,
            TransactionType = TransactionType.Deposit,
            AmountDkk = refundInfo.PriceDkk,
            Status = TransactionStatus.Approved,
            MobilePayReference = null,
            BoardId = boardId,
            ProcessedBy = adminId,
            ProcessedAt = nowUtc,
            CreatedAt = nowUtc,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null,
        });
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task ProcessSubscriptionsForGameAsync(Guid gameId)
    {
        var nowUtc = DateTime.UtcNow;

        var gameState = await _context.Games
            .AsNoTracking()
            .Where(g => g.Id == gameId)
            .Select(g => new
            {
                g.Id,
                g.IsActive,
                g.NumbersPublishedAt,
                g.GuessDeadline,
                g.StartDate
            })
            .SingleOrDefaultAsync();

        if (gameState is null)
        {
            throw new KeyNotFoundException($"Game with id {gameId} doesn't exist");
        }

        if (!gameState.IsActive)
        {
            throw new InvalidOperationException("Cannot process subscriptions for an inactive game");
        }

        if (gameState.NumbersPublishedAt is not null)
        {
            throw new InvalidOperationException("Cannot process subscriptions for a finished game");
        }

        if (nowUtc >= gameState.GuessDeadline)
        {
            throw new InvalidOperationException("Cannot process subscriptions after the guessing deadline");
        }
        
        var subscriptions = await _context.BoardSubscriptions
            .Include(bs => bs.Numbers)
            .Include(bs => bs.Boards)
            .Where(bs => bs.IsActive)
            .Where(bs => bs.StartGame.StartDate <= gameState.StartDate)
            //if total games == null the subscription lasts as long as the balance does
            .Where(bs => bs.TotalGames == null || bs.GamesAlreadyPlayed < bs.TotalGames)
            .Where(bs => !bs.Boards.Any(b => b.GameId == gameId))
            .ToListAsync();

        if (subscriptions.Count == 0)
        {
            return;
        }

        foreach (var subscription in subscriptions)
        {
            var numbers = subscription.Numbers
                .Select(n => n.Number)
                .ToList();
            //TODO might be redundant validation if the eventual create board subscription method also validates the numbers.
            ValidateBoardNumbers(numbers);

            var numberCount = numbers.Count;
            var price = subscription.PricePerGameDkk;
            var expectedPrice = CalculatePrice(numberCount);
            if (expectedPrice != price)
            {
                //TODO figure out what I want to do here
                continue;
            }
            
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            var acquired = await TransactionLocks
                .TryAcquireUserTransactionLockAsync(_context, subscription.PlayerId);

            if (!acquired)
            {
                throw new InvalidOperationException("Another transaction is already in progress");
            }
            
            var balance = await GetPlayerBalanceInternalAsync(subscription.PlayerId);
            if (balance < price)
            {
                subscription.IsActive = false;
                subscription.CancelledAt = nowUtc;
                subscription.UpdatedAt = nowUtc;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                continue;
            }

            var board = new Board
            {
                Id = Guid.NewGuid(),
                GameId = gameId,
                PlayerId = subscription.PlayerId,
                NumberCount = numberCount,
                PriceDkk = price,
                IsWinningBoard = false,
                SubscriptionId = subscription.Id,
                CreatedAt = nowUtc,
                UpdatedAt = null,
                IsDeleted = false,
                DeletedAt = null,
            };

            foreach (var number in numbers.OrderBy(n => n))
            {
                board.Numbers.Add(new BoardNumber
                {
                    Number = number,
                });
            }
            
            _context.Boards.Add(board);

            _context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                PlayerId = subscription.PlayerId,
                TransactionType = TransactionType.Purchase,
                AmountDkk = price,
                Status = TransactionStatus.Approved,
                MobilePayReference = null,
                BoardId = board.Id,
                ProcessedBy = null,
                ProcessedByUser = null,
                ProcessedAt = null,
                CreatedAt = nowUtc,
                UpdatedAt = null,
                IsDeleted = false,
                DeletedAt = null,
            });
            
            subscription.GamesAlreadyPlayed++;
            subscription.UpdatedAt = nowUtc;

            if (subscription.TotalGames is not null && subscription.GamesAlreadyPlayed >= subscription.TotalGames.Value)
            {
                subscription.IsActive = false;
                subscription.CancelledAt ??= nowUtc;
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
using System.Linq.Expressions;
using Api.DTOs.Responses.PlayerResponses;
using Api.DTOs.Responses.TransactionResponses;
using DataAccess.Entities;

namespace Api.DTOs.Util;

public static class EntityToDtoMapper
{
    public static ApplicationUserDto ToDto(this ApplicationUser user)
    {
        return new ApplicationUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsActivePlayer = user.IsActivePlayer,
            //can be set by overload, but generally not needed
            Role = string.Empty, 
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    }
    
    //overload that accepts roles
    public static ApplicationUserDto ToDto(this ApplicationUser user, IList<string> roles)
    {
        var dto = user.ToDto();
        dto.Role = roles.FirstOrDefault() ?? string.Empty;
        return dto;
    }
    
    //Project user to dto
    public static readonly Expression<Func<ApplicationUser, ApplicationUserDto>> ApplicationUserToDto =
        user => new ApplicationUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            IsActivePlayer = user.IsActivePlayer,
            Role = string.Empty,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };

    public static GameDto ToDto(this Game game)
    {
        return new GameDto
        {
            Id = game.Id,
            WeekNumber = game.WeekNumber,
            Year = game.Year,
            StartDate = game.StartDate,
            EndDate = game.EndDate,
            GuessDeadline = game.GuessDeadline,
            IsActive = game.IsActive,
            WinningNumber1 = game.WinningNumber1,
            WinningNumber2 = game.WinningNumber2,
            WinningNumber3 = game.WinningNumber3,
            NumbersPublishedAt = game.NumbersPublishedAt,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt,
        };
    }
    
    //Projection Game to GameDto
    public static readonly Expression<Func<Game, GameDto>> GameToDto = game => new GameDto
    {
        Id = game.Id,
        WeekNumber = game.WeekNumber,
        Year = game.Year,
        StartDate = game.StartDate,
        EndDate = game.EndDate,
        GuessDeadline = game.GuessDeadline,
        IsActive = game.IsActive,
        WinningNumber1 = game.WinningNumber1,
        WinningNumber2 = game.WinningNumber2,
        WinningNumber3 = game.WinningNumber3,
        NumbersPublishedAt = game.NumbersPublishedAt,
        CreatedAt = game.CreatedAt,
        UpdatedAt = game.UpdatedAt,
    };

    public static BoardDto ToDto(this Board board)
    {
        return new BoardDto
        {
            Id = board.Id,
            GameId = board.GameId,
            PlayerId = board.PlayerId,
            NumberCount = board.NumberCount,
            PriceDkk = board.PriceDkk,
            IsWinningBoard = board.IsWinningBoard,
            SubscriptionId = board.SubscriptionId,
            CreatedAt = board.CreatedAt,
            UpdatedAt = board.UpdatedAt,
            //potential N+1 risk, if I ever touch numbers
            Numbers = board.Numbers?.Select(n => n.Number).ToList() ?? []
        };
    }
    //overloaded with relations
    public static BoardDto ToDto(this Board board, bool includeRelations)
    {
        var dto = board.ToDto();
        if (!includeRelations) return dto;
        
        dto.Game = board.Game?.ToDto();
        dto.Player = board.Player?.ToDto();
        return dto;
    }
    
    //Project Board to BoardDto
    public static readonly Expression<Func<Board, BoardDto>> BoardToDto = board => new BoardDto
    {
        Id = board.Id,
        GameId = board.GameId,
        PlayerId = board.PlayerId,
        PlayerName = board.Player.FullName,
        NumberCount = board.NumberCount,
        PriceDkk = board.PriceDkk,
        IsWinningBoard = board.IsWinningBoard,
        SubscriptionId = board.SubscriptionId,
        CreatedAt = board.CreatedAt,
        UpdatedAt = board.UpdatedAt,
        Numbers = board.Numbers
            .OrderBy(n => n.Number)
            .Select(n => n.Number)
            .ToList(),
    };
    public static readonly Expression<Func<Board, BoardDto>> BoardToDtoWithGame = board => new BoardDto
    {
        Id = board.Id,
        GameId = board.GameId,
        PlayerId = board.PlayerId,
        PlayerName = board.Player.FullName,

        NumberCount = board.NumberCount,
        PriceDkk = board.PriceDkk,
        IsWinningBoard = board.IsWinningBoard,
        SubscriptionId = board.SubscriptionId,
        CreatedAt = board.CreatedAt,
        UpdatedAt = board.UpdatedAt,

        Numbers = board.Numbers
            .OrderBy(n => n.Number)
            .Select(n => n.Number)
            .ToList(),

        Game = new GameDto
        {
            Id = board.Game.Id,
            WeekNumber = board.Game.WeekNumber,
            Year = board.Game.Year,
            StartDate = board.Game.StartDate,
            EndDate = board.Game.EndDate,
            GuessDeadline = board.Game.GuessDeadline,
            IsActive = board.Game.IsActive,
            WinningNumber1 = board.Game.WinningNumber1,
            WinningNumber2 = board.Game.WinningNumber2,
            WinningNumber3 = board.Game.WinningNumber3,
            NumbersPublishedAt = board.Game.NumbersPublishedAt,
            CreatedAt = board.Game.CreatedAt,
            UpdatedAt = board.Game.UpdatedAt,
        }
    };

    public static BoardSubscriptionDto ToDto(this BoardSubscription subscription)
    {
        return new BoardSubscriptionDto
        {
            Id = subscription.Id,
            PlayerId = subscription.PlayerId,
            PricePerGameDkk = subscription.PricePerGameDkk,
            StartGameId = subscription.StartGameId,
            TotalGames = subscription.TotalGames,
            GamesAlreadyPlayed = subscription.GamesAlreadyPlayed,
            IsActive = subscription.IsActive,
            CancelledAt = subscription.CancelledAt,
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt,
            Numbers = subscription.Numbers?.Select(n => n.Number).ToList() ?? []
        };
    }
    //overloaded with relations
    public static BoardSubscriptionDto ToDto(this BoardSubscription subscription, bool includeRelations)
    {
        var dto = subscription.ToDto();
        if (!includeRelations) return dto;

        dto.StartGame = subscription.StartGame?.ToDto();
        dto.Player = subscription.Player?.ToDto();
        return dto;
    }

    public static TransactionDto ToDto(this Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            PlayerId = transaction.PlayerId,
            TransactionType = transaction.TransactionType,
            AmountDkk = transaction.AmountDkk,
            Status = transaction.Status,
            MobilePayReference = transaction.MobilePayReference,
            BoardId = transaction.BoardId,
            ProcessedAt = transaction.ProcessedAt,
            ProcessedBy = transaction.ProcessedBy,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
        };
    }
    //overloaded with relations
    public static TransactionDto ToDto(this Transaction transaction, bool includeRelations)
    {
        var dto = transaction.ToDto();
        if (!includeRelations) return dto;

        dto.Player = transaction.Player?.ToDto();
        dto.Board = transaction.Board?.ToDto();
        return dto;
    }
    
    //full transaction dto including player and board relations, very bloated
    public static readonly Expression<Func<Transaction, TransactionDto>> TransactionToDto =
        t => new TransactionDto
        {
            Id = t.Id,
            PlayerId = t.PlayerId,
            TransactionType = t.TransactionType,
            AmountDkk = t.AmountDkk,
            Status = t.Status,
            MobilePayReference = t.MobilePayReference,
            BoardId = t.BoardId,
            ProcessedAt = t.ProcessedAt,
            ProcessedBy = t.ProcessedBy,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,

            Player = new ApplicationUserDto
            {
                Id = t.Player.Id,
                FullName = t.Player.FullName,
                Email = t.Player.Email ?? string.Empty,
                PhoneNumber = t.Player.PhoneNumber ?? string.Empty,
                IsActivePlayer = t.Player.IsActivePlayer,
                Role = string.Empty,
                CreatedAt = t.Player.CreatedAt,
                UpdatedAt = t.Player.UpdatedAt,
            },

            Board = t.Board == null
                ? null
                : new BoardDto
                {
                    Id = t.Board.Id,
                    GameId = t.Board.GameId,
                    PlayerId = t.Board.PlayerId,
                    PlayerName = t.Board.Player.FullName,
                    NumberCount = t.Board.NumberCount,
                    PriceDkk = t.Board.PriceDkk,
                    IsWinningBoard = t.Board.IsWinningBoard,
                    SubscriptionId = t.Board.SubscriptionId,
                    CreatedAt = t.Board.CreatedAt,
                    UpdatedAt = t.Board.UpdatedAt,
                    Numbers = t.Board.Numbers
                        .OrderBy(n => n.Number)
                        .Select(n => n.Number)
                        .ToList()
                }
        };
    
    //projection for pending deposit list items
    public static readonly Expression<Func<Transaction, PendingDepositsListItemDto>> PendingTransactionListItemToDto =
        t => new PendingDepositsListItemDto
        {
            TransactionId = t.Id,
            PlayerId = t.PlayerId,
            PlayerFullName = t.Player.FullName,
            AmountDkk = t.AmountDkk,
            MobilePayReference = t.MobilePayReference!,
            CreatedAt = t.CreatedAt,
        };
    
    //projection for transaction history list item, grabs board id to differentiate between purchase and deposit
    public static readonly Expression<Func<Transaction, TransactionHistoryListItemDto>> TransactionHistoryListItemToDto =
        t => new TransactionHistoryListItemDto
        {
            TransactionId = t.Id,
            Status = t.Status,
            PlayerId = t.PlayerId,
            MobilePayReference = t.MobilePayReference,
            AmountDkk = t.AmountDkk,
            ProcessedAt = t.ProcessedAt,
            CreatedAt = t.CreatedAt,
            BoardId = t.BoardId,
        };
}
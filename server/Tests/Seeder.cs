using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tests.DatabaseUtil;

namespace Tests;

public interface ISeeder
{
    Task Clear();

    Task EnsureUsersSeededOnceAsync();
    Task Seed();

    Task<Game> SeedActiveGameAsync(DateTime? guessDeadlineUtc = null);

    Task<Transaction> SeedPendingDepositAsync(
        Guid playerId,
        int amountDkk = 123,
        string mobilePayReference = "111222333",
        DateTime? createdAtUtc = null);

    Task<Transaction> SeedApprovedPurchaseAsync(
        Guid playerId,
        int amountDkk = 20,
        DateTime? createdAtUtc = null);

    Task<Game> SeedGameAsync(
        int weekNumber, 
        int year, 
        bool isActive, 
        DateTime? numbersPublishedAt = null,
        DateTime? guessDeadlineUtc = null, 
        DateTime? startDateUtc = null, 
        DateTime? endDateUtc = null);
    
    Task<Board> SeedBoardAsync(
        Guid playerId,
        Guid gameId,
        DateTime createdAtUtc,
        int numberCount = 5,
        int priceDkk = 20,
        bool isWinningBoard = false);
}

public class Seeder(
    MyDbContext context,
    DatabaseInitializer _initializer,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager
) : ISeeder
{
    public async Task Clear()
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        await context.BoardNumbers.IgnoreQueryFilters().ExecuteDeleteAsync();

        await context.BoardSubscriptionNumbers.IgnoreQueryFilters().ExecuteDeleteAsync();

        await context.Transactions.IgnoreQueryFilters().ExecuteDeleteAsync();

        await context.Boards.IgnoreQueryFilters().ExecuteDeleteAsync();

        await context.BoardSubscriptions.IgnoreQueryFilters().ExecuteDeleteAsync();

        await context.Games.IgnoreQueryFilters().ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }


    private static int _usersSeeded;

    public async Task EnsureUsersSeededOnceAsync()
    {
        await context.Database.EnsureCreatedAsync();

        if (Interlocked.Exchange(ref _usersSeeded, 1) == 1)
        {
            return;
        }

        await EnsureRoleAsync("Admin");
        await EnsureRoleAsync("Player");

        await EnsureUserAsync(
            id: SeedUsers.AdminId,
            email: SeedUsers.AdminEmail,
            fullName: "Admin",
            isActivePlayer: false,
            password: "123456",
            role: "Admin"
        );

        await EnsureUserAsync(
            id: SeedUsers.ActiveRichId,
            email: SeedUsers.ActiveRichEmail,
            fullName: "Active Rich",
            isActivePlayer: true,
            password: "123456",
            role: "Player"
        );

        await EnsureUserAsync(
            id: SeedUsers.ActivePoorId,
            email: SeedUsers.ActivePoorEmail,
            fullName: "Active Poor",
            isActivePlayer: true,
            password: "123456",
            role: "Player"
        );

        await EnsureUserAsync(
            id: SeedUsers.InactiveId,
            email: SeedUsers.InactiveEmail,
            fullName: "Inactive Player",
            isActivePlayer: false,
            password: "123456",
            role: "Player"
        );
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var res = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        if (!res.Succeeded)
            throw new InvalidOperationException("Failed creating role: " +
                                                string.Join(", ", res.Errors.Select(e => e.Description)));
    }

    private async Task<ApplicationUser> EnsureUserAsync(
        Guid id,
        string email,
        string fullName,
        bool isActivePlayer,
        string password,
        string role
    )
    {
        // Find by email (normalized lookup handled by Identity)
        var existing = await userManager.FindByEmailAsync(email);

        if (existing != null)
        {
            // Ensure your custom fields are correct
            existing.FullName = fullName;
            existing.IsActivePlayer = isActivePlayer;

            // Keep soft-delete fields sane for tests
            existing.IsDeleted = false;
            existing.DeletedAt = null;

            // Don't stomp CreatedAt if you already have it
            if (existing.CreatedAt == default)
                existing.CreatedAt = DateTime.UtcNow;

            existing.UpdatedAt = DateTime.UtcNow;

            var upd = await userManager.UpdateAsync(existing);
            if (!upd.Succeeded)
                throw new InvalidOperationException("Failed updating user: " +
                                                    string.Join(", ", upd.Errors.Select(e => e.Description)));

            if (!await userManager.IsInRoleAsync(existing, role))
            {
                var addRole = await userManager.AddToRoleAsync(existing, role);
                if (!addRole.Succeeded)
                    throw new InvalidOperationException("Failed adding role: " +
                                                        string.Join(", ", addRole.Errors.Select(e => e.Description)));
            }

            return existing;
        }

        var nowUtc = DateTime.UtcNow;

        var user = new ApplicationUser
        {
            Id = id,
            UserName = email,
            Email = email,
            EmailConfirmed = true,

            FullName = fullName,
            IsActivePlayer = isActivePlayer,

            CreatedAt = nowUtc,
            UpdatedAt = null,

            IsDeleted = false,
            DeletedAt = null,
        };

        var create = await userManager.CreateAsync(user, password);
        if (!create.Succeeded)
            throw new InvalidOperationException("Failed creating user: " +
                                                string.Join(", ", create.Errors.Select(e => e.Description)));

        var add = await userManager.AddToRoleAsync(user, role);
        if (!add.Succeeded)
            throw new InvalidOperationException("Failed adding role: " +
                                                string.Join(", ", add.Errors.Select(e => e.Description)));

        return user;
    }


    public async Task Seed()
    {
        await EnsureUsersSeededOnceAsync();

        var nowUtc = DateTime.UtcNow;

        context.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            PlayerId = SeedUsers.ActiveRichId,
            Player = null!,
            TransactionType = TransactionType.Deposit,
            AmountDkk = 500,
            Status = TransactionStatus.Approved,
            MobilePayReference = null,

            BoardId = null,
            Board = null,

            ProcessedBy = SeedUsers.AdminId,
            ProcessedByUser = null!,
            ProcessedAt = nowUtc,

            CreatedAt = nowUtc,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null,
        });

        await context.SaveChangesAsync();
    }

    public async Task<Game> SeedActiveGameAsync(DateTime? guessDeadlineUtc = null)
    {
        var now = DateTime.UtcNow;

        var game = new Game
        {
            Id = Guid.NewGuid(),
            WeekNumber = 1,
            Year = now.Year,
            StartDate = now.Date,
            EndDate = now.Date.AddDays(7),

            GuessDeadline = guessDeadlineUtc ?? now.AddHours(2),
            IsActive = true,

            NumbersPublishedAt = null,
            WinningNumber1 = null,
            WinningNumber2 = null,
            WinningNumber3 = null,

            CreatedAt = now,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();

        return game;
    }

    public async Task<Transaction> SeedPendingDepositAsync(
        Guid playerId, 
        int amountDkk = 123,
        string mobilePayReference = "111222333",
        DateTime? createdAtUtc = null)
    {
        var now = createdAtUtc ?? DateTime.UtcNow;

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Player = null!,
            TransactionType = TransactionType.Deposit,
            AmountDkk = amountDkk,
            Status = TransactionStatus.Pending,
            MobilePayReference = mobilePayReference,

            BoardId = null,
            Board = null,

            ProcessedBy = null,
            ProcessedByUser = null,
            ProcessedAt = null,

            CreatedAt = now,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null
        };
        
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction> SeedApprovedPurchaseAsync(
        Guid playerId, 
        int amountDkk = 20, 
        DateTime? createdAtUtc = null)
    {
        var now = createdAtUtc ?? DateTime.UtcNow;

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Player = null!,
            TransactionType = TransactionType.Purchase,
            AmountDkk = amountDkk,
            Status = TransactionStatus.Approved,
            MobilePayReference = null,

            BoardId = null,
            Board = null,
            
            ProcessedBy = SeedUsers.AdminId,
            ProcessedByUser = null,
            ProcessedAt = now,

            CreatedAt = now,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null
        };
        
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Game> SeedGameAsync(int weekNumber, int year, bool isActive, DateTime? numbersPublishedAt = null,
        DateTime? guessDeadlineUtc = null, DateTime? startDateUtc = null, DateTime? endDateUtc = null)
    {
        var now = DateTime.UtcNow;

        var start = (startDateUtc ?? now).Date;
        var end = (endDateUtc ?? start.AddDays(7)).Date;

        var game = new Game
        {
            Id = Guid.NewGuid(),
            WeekNumber = weekNumber,
            Year = year,

            StartDate = start,
            EndDate = end,

            GuessDeadline = guessDeadlineUtc ?? now.AddHours(2),
            IsActive = isActive,

            NumbersPublishedAt = numbersPublishedAt,
            WinningNumber1 = null,
            WinningNumber2 = null,
            WinningNumber3 = null,

            CreatedAt = now,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();
        return game;
    }

    public async Task<Board> SeedBoardAsync(
        Guid playerId, 
        Guid gameId, 
        DateTime createdAtUtc,
        int numberCount = 5,
        int priceDkk = 20,
        bool isWinningBoard = false)
    {
        var board = new Board
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            Player = null!,
            GameId = gameId,
            Game = null!,

            NumberCount = numberCount,
            PriceDkk = priceDkk,
            IsWinningBoard = isWinningBoard,

            SubscriptionId = null,
            Subscription = null,

            CreatedAt = createdAtUtc,
            UpdatedAt = null,
            IsDeleted = false,
            DeletedAt = null
        };
        context.Boards.Add(board);
        await context.SaveChangesAsync();
        return board;
    }
}
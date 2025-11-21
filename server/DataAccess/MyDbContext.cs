using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class MyDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardNumber>  BoardNumbers => Set<BoardNumber>();
    public DbSet<BoardSubscription> BoardSubscriptions => Set<BoardSubscription>();
    public DbSet<BoardSubscriptionNumber> BoardSubscriptionNumbers => Set<BoardSubscriptionNumber>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //composite keys -- numbers to boards
        builder.Entity<BoardNumber>()
            .HasKey(bn => new { bn.BoardId, bn.Number });
        builder.Entity<BoardSubscriptionNumber>()
            .HasKey(bsn => new {bsn.BoardSubscriptionId, bsn.Number});
        
        //Table relationships
        
        //ApplicationUser 1 - * Board
        builder.Entity<Board>()
            .HasOne(b => b.Player)
            .WithMany(p => p.Boards)
            .HasForeignKey(b => b.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //Game 1 - * Board
        builder.Entity<Board>()
            .HasOne(b => b.Game)
            .WithMany(g => g.Boards)
            .HasForeignKey(b => b.GameId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //Board 1 - * BoardNumber
        builder.Entity<BoardNumber>()
            .HasOne(bn => bn.Board)
            .WithMany(b => b.Numbers)
            .HasForeignKey(bn => bn.BoardId);
        
        //BoardSubscription 1 - * Board
        builder.Entity<Board>()
            .HasOne(b => b.Subscription)
            .WithMany(s => s.Boards)
            .HasForeignKey(b => b.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);
        
        //BoardSubscription 1 - * BoardSubscriptionNumber
        builder.Entity<BoardSubscriptionNumber>()
            .HasOne(bsn => bsn.BoardSubscription)
            .WithMany(bs => bs.Numbers)
            .HasForeignKey(bsn => bsn.BoardSubscriptionId);
        
        //Game 1 - * BoardSubscription
        builder.Entity<BoardSubscription>()
            .HasOne(bs => bs.StartGame)
            .WithMany(g => g.BoardSubscriptions)
            .HasForeignKey(bs => bs.StartGameId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //ApplicationUser (player) 1 - * BoardSubscription
        builder.Entity<BoardSubscription>()
            .HasOne(bs => bs.Player)
            .WithMany(p => p.BoardSubscriptions)
            .HasForeignKey(bs => bs.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //ApplicationUser 1 - * Transaction
        builder.Entity<Transaction>()
            .HasOne(t => t.Player)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //Board 1 - *  Transaction -- 1-* for refunding/cancelling purposes.
        builder.Entity<Transaction>()
            .HasOne(t => t.Board)
            .WithMany(b => b.Transactions)
            .HasForeignKey(t => t.BoardId)
            .OnDelete(DeleteBehavior.Restrict);
        
        //ApplicationUser (admin) 1 - * Transactions
        builder.Entity<Transaction>()
            .HasOne(t => t.ProcessedByUser)
            .WithMany(u => u.ProcessedTransactions)
            .HasForeignKey(t => t.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);
        
        //soft-delete filters
        builder.Entity<ApplicationUser>()
            .HasQueryFilter(u => !u.IsDeleted);
        
        builder.Entity<Game>()
            .HasQueryFilter(g => !g.IsDeleted);
        
        builder.Entity<Board>()
            .HasQueryFilter(b => !b.IsDeleted);
        
        builder.Entity<BoardSubscription>()
            .HasQueryFilter(b => !b.IsDeleted);
        
        builder.Entity<Transaction>()
            .HasQueryFilter(b => !b.IsDeleted);
        
        builder.Entity<BoardNumber>()
            .HasQueryFilter(b => !b.IsDeleted);
        
        builder.Entity<BoardSubscriptionNumber>()
            .HasQueryFilter(b => !b.IsDeleted);

    }   
}
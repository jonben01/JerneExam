using Microsoft.AspNetCore.Identity;

namespace DataAccess.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;
    //defaulting to false - new accounts must be verified to play
    public bool IsActivePlayer { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<BoardSubscription> BoardSubscriptions { get; set; } = new List<BoardSubscription>();
    
    public ICollection<Transaction> ProcessedTransactions { get; set; } = new List<Transaction>();
}
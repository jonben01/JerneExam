namespace DataAccess.Entities;

public class Board
{
    public Guid Id { get; set; }
    
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;
    
    public Guid PlayerId { get; set; }
    public ApplicationUser Player { get; set; } = null!;
    
    public int NumberCount { get; set; }
    public int PriceDkk { get; set; }
    public bool IsWinningBoard { get; set; }
    
    public Guid? SubscriptionId { get; set; }
    public BoardSubscription? Subscription { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public ICollection<BoardNumber> Numbers { get; set; } = new List<BoardNumber>();
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    
}
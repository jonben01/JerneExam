namespace DataAccess.Entities;

public class BoardSubscription
{
    public Guid Id { get; set; }
    
    public Guid PlayerId { get; set; }
    public ApplicationUser Player { get; set; } = null!;
    
    public int PricePerGameDkk { get; set; }
    
    public Guid StartGameId { get; set; }
    public Game StartGame { get; set; } = null!;
    
    public int? TotalGames { get; set; }
    public int GamesAlreadyPlayed { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime? CancelledAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public ICollection<BoardSubscriptionNumber> Numbers { get; set; } = new List<BoardSubscriptionNumber>();
    public ICollection<Board> Boards { get; set; } = new List<Board>();
}
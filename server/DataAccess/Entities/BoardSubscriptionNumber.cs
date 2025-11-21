namespace DataAccess.Entities;

public class BoardSubscriptionNumber
{
    public Guid BoardSubscriptionId { get; set; }
    public BoardSubscription BoardSubscription { get; set; } = null!;
    
    public int Number { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
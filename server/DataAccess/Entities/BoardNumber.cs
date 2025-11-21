namespace DataAccess.Entities;

public class BoardNumber
{
    public Guid BoardId { get; set; }
    public Board Board { get; set; } = null!;
    
    public int Number { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
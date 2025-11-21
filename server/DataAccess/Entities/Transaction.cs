namespace DataAccess.Entities;

public enum TransactionType : byte
{
    Deposit = 0,
    Purchase = 1
}

public enum TransactionStatus : byte
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class Transaction
{
    public Guid Id { get; set; }
    
    public Guid PlayerId { get; set; }
    public ApplicationUser Player { get; set; } = null!;
    
    public TransactionType TransactionType { get; set; }
    public int AmountDkk { get; set; }
    public TransactionStatus Status { get; set; }
    
    //Only relevant for deposits.
    public string? MobilePayReference { get; set; }
    
    //Only relevant for board purchases.
    public Guid? BoardId { get; set; }
    public Board? Board { get; set; } 
    
    //Who approved or rejected transaction.
    public Guid? ProcessedBy { get; set; }
    public ApplicationUser? ProcessedByUser { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
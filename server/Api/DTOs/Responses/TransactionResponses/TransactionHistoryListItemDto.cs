using DataAccess.Entities;

namespace Api.DTOs.Responses.TransactionResponses;

public class TransactionHistoryListItemDto
{
    public Guid TransactionId { get; set; }
    
    public TransactionStatus Status { get; set; }
    
    public Guid PlayerId { get; set; }
    
    //Required for deposits
    public string? MobilePayReference { get; set; }
    
    //"Required" for board purchases
    public Guid? BoardId { get; set; }
    
    public int AmountDkk { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
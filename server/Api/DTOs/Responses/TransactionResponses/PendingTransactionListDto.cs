namespace Api.DTOs.Responses.TransactionResponses;

public class PendingDepositsListItemDto
{
    public Guid TransactionId  { get; set; }
    
    public Guid PlayerId { get; set; }
    
    public required string PlayerFullName { get; set; }
    
    public required string MobilePayReference { get; set; }
    
    public int AmountDkk { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
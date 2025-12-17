using Api.DTOs.Responses.PlayerResponses;
using DataAccess.Entities;

namespace Api.DTOs.Responses.TransactionResponses;

public class TransactionDto
{
    public Guid Id { get; set; }
    
    public Guid PlayerId { get; set; }
    
    public TransactionType TransactionType { get; set; }
    
    public int AmountDkk { get; set; }
    
    public TransactionStatus Status { get; set; }
    
    //Only relevant for deposits.
    public string? MobilePayReference { get; set; }
    
    //Only relevant for board purchases.
    public Guid? BoardId { get; set; }
    
    public Guid? ProcessedBy { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    //nested data to avoid repeat db calls, when relevant.
    public ApplicationUserDto? Player { get; set; }
    public BoardDto? Board { get; set; }
    
    
}
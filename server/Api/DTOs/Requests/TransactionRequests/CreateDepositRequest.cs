using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.TransactionRequests;

public record CreateDepositRequest
{
    [Required, Range(1, int.MaxValue, ErrorMessage = "Amount must be positive")]
    public required int AmountDkk { get; set; }
    
    [Required, StringLength(20)]
    [RegularExpression(@"^\d{1,20}$", ErrorMessage = "MobilePayReference must be numbers only")]
    public required string MobilePayReference { get; set; }
}
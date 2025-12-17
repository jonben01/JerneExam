using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.AuthRequests;

public record RegisterPlayerRequest
{
    
    [Required ,EmailAddress, MaxLength(150)]
    public required string Email { get; set; }
    
    [Required ,MaxLength(100)]
    public required string FullName { get; set; }
    
    [Required,Phone]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "Phone number must be 8 digits.")]
    public required string PhoneNumber { get; set; }
}
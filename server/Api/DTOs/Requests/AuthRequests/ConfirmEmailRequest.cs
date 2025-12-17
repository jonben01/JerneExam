using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.AuthRequests;

public record ConfirmEmailRequest
{
    [Required]
    public required string UserId { get; set; }
    
    [Required]
    public required string Token { get; set; }
    
    [Required, MinLength(6)]
    public required string Password { get; set; }
}
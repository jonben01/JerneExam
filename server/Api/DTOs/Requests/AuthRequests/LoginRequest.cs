using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.AuthRequests;

public record LoginRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }
    
    [Required,MinLength(6)]
    public required string Password { get; set; }
}
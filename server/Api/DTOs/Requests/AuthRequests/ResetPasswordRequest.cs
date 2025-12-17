using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.AuthRequests;

public record ResetPasswordRequest
{
    //this should arguably be combined with confirm email request (same fields)
    [Required]
    public required string UserId { get; set; }
    
    [Required]
    public required string Token { get; set; }
    
    [Required, MinLength(6)]
    public required string NewPassword { get; set; }
}
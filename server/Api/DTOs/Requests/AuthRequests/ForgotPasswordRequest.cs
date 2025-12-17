using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.AuthRequests;

public record ForgotPasswordRequest
{
    [Required,  EmailAddress]
    public required string Email { get; set; }
}
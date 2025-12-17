using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.PlayerRequests;

public class UpdatePlayerAdminRequest
{
    [MaxLength(100)]
    public string? FullName { get; set; } = null!;
    
    [EmailAddress, MaxLength(100)]
    public string? Email { get; set; } = null!;
    
    [Phone, MaxLength(15)]
    public string? PhoneNumber { get; set; } = null!;
}
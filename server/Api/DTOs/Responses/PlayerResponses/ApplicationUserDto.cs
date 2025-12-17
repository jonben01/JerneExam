
namespace Api.DTOs.Responses.PlayerResponses;

public class ApplicationUserDto()
{
    public Guid Id { get; set; }
    
    public string FullName { get; set; } = null!;
    
    public string Email { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;
    
    public bool IsActivePlayer { get; set; }
    
    public string Role { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}
namespace Api.DTOs.Responses.PlayerResponses;

public class ApplicationUserListItemDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;
    
    public string Email { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;
    
    public bool IsActivePlayer { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
}
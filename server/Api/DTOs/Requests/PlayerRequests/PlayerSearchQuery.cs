using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.PlayerRequests;

public record PlayerSearchQuery
{
    [MaxLength(50)]
    public string? Query { get; set; }
    
    public bool? IsActive { get; set; }

    public bool IncludeDeleted { get; set; } = false;
    
    //TODO definitely add pagination
}
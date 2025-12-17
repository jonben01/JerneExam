using Api.DTOs.Requests.PlayerRequests;
using Api.DTOs.Responses.PlayerResponses;

namespace Api.Services.Interfaces;

public interface IPlayerService
{
    //Player-only
    Task<ApplicationUserDto> GetSelfAsync(Guid currentUserId, CancellationToken ct = default);
    
    Task<ApplicationUserDto> UpdateSelfAsync(
        Guid currentUserId, 
        UpdatePlayerRequest request, 
        CancellationToken ct = default);
    
    //Admin-only
    Task<ApplicationUserDto> UpdatePlayerAsync(
        Guid playerId, 
        UpdatePlayerAdminRequest request, 
        CancellationToken ct = default);
    
    Task<ApplicationUserDto> GetByIdAsync(Guid userId, bool includeDeleted = false, CancellationToken ct = default);
        
    Task<IReadOnlyList<ApplicationUserListItemDto>> SearchAsync(
        PlayerSearchQuery query, 
        CancellationToken ct = default); 
    
    Task<ApplicationUserDto> SetActivityStatusAsync(
        Guid playerId, 
        bool status, 
        CancellationToken ct = default);
    
    Task SoftDeleteAsync(Guid playerId, CancellationToken ct = default);
    
    Task RestoreAsync(Guid playerId, CancellationToken ct = default);
}

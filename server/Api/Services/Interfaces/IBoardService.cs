using Api.DTOs;
using Api.DTOs.Requests;
using Api.DTOs.Requests.BoardRequests;

namespace Api.Services.Interfaces;

public interface IBoardService
{
    //Player actions
    Task<(BoardDto, int newBalance)> PurchaseBoardAsync(PurchaseBoardRequest request, Guid userId);
    Task<BoardDto> GetBoardByIdAsync(Guid boardId);
    Task<List<BoardDto>> GetPlayerBoardsAsync(Guid userId, Guid? gameId = null);
    Task<List<BoardDto>> GetPlayerWinningBoardsAsync(Guid userId);
    Task<bool> CanPlayerAffordBoardAsync(Guid userId, int numberCount);
    
    //Admin actions
    Task<List<BoardDto>> GetBoardsForGameAsync(Guid gameId, bool includePlayerInfo = false);
    Task<List<BoardDto>> GetWinningBoardsForGameAsync(Guid gameId);
    Task DeleteBoardAsync(Guid boardId, Guid adminId);
    
    //System related
    Task ProcessSubscriptionsForGameAsync(Guid gameId);
}
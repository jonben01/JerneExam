using Api.DTOs;
using Api.DTOs.Requests.GameRequests;

namespace Api.Services.Interfaces;

public interface IGameService
{
    //gets
    Task<GameDto> GetActiveGameAsync(); 
    Task<GameDto> GetGameByIdAsync(Guid gameId);
    //TODO implement pagination on game history - 10 years = 520 games
    Task<List<GameDto>> GetGameHistoryAsync();
    
    //admin
    Task<GameDto> PublishWinningNumbersAndEndGameAsync(PublishWinningNumbersRequest request);
    Task<GameAdminOverviewDto> GetGameAdminOverviewAsync(Guid gameId);
    
    //info gets
    Task<List<BoardDto>> GetWinningBoardsForGameAsync(Guid gameId);
    Task<int> GetTotalWinningBoardCountAsync(Guid gameId);
    
    Task<List<GameStatsDto>> GetAllGameStatsAsync(CancellationToken ct = default);
    Task<GameStatsDto> GetGameStatsByIdAsync(Guid gameId);
    
    //validation, checks if game is active/not ended/before deadline
    Task<bool> CanAcceptNewBoardsAsync(Guid gameId);
    
    //Seeding, should maybe move since its a one time thing.
    Task SeedGamesAsync();
    
}
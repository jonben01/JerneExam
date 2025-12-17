using Api.DTOs;
using Api.DTOs.Requests.GameRequests;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GameController(IGameService gameService, IBoardService boardService) : ControllerBase
{
    [HttpGet]
    [Route(nameof(GetActiveGame))]
    public async Task<ActionResult<GameDto>> GetActiveGame()
    {
        return await gameService.GetActiveGameAsync();
    }

    [HttpGet]
    [Route(nameof(GetGameById) + "/{gameId:guid}" )]
    public async Task<ActionResult<GameDto>> GetGameById(Guid gameId)
    {
        return await gameService.GetGameByIdAsync(gameId);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetGameHistory))]
    public async Task<ActionResult<List<GameDto>>> GetGameHistory()
    {
        return await gameService.GetGameHistoryAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route(nameof(PublishWinningNumbersAndEndGame))]
    public async Task<ActionResult<GameDto>> PublishWinningNumbersAndEndGame(
        [FromBody] PublishWinningNumbersRequest request)
    {
        var endedGame =  await gameService.PublishWinningNumbersAndEndGameAsync(request);
        
        var nextGame = await gameService.GetActiveGameAsync();
        
        //start all subscription games after the next game has been activated
        await boardService.ProcessSubscriptionsForGameAsync(nextGame.Id);
        
        return endedGame;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetGameAdminOverview) + "/{gameId:guid}" )]
    public async Task<ActionResult<GameAdminOverviewDto>> GetGameAdminOverview(Guid gameId)
    {
        return await gameService.GetGameAdminOverviewAsync(gameId);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetWinningBoardsForGame) + "/{gameId:guid}" )]
    public async Task<ActionResult<List<BoardDto>>> GetWinningBoardsForGame(Guid gameId)
    {
        return await gameService.GetWinningBoardsForGameAsync(gameId);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetTotalWinningBoardCount) + "/{gameId:guid}" )]
    public async Task<ActionResult<int>> GetTotalWinningBoardCount(Guid gameId)
    {
        return await gameService.GetTotalWinningBoardCountAsync(gameId);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetAllGameStats))]
    public async Task<ActionResult<List<GameStatsDto>>> GetAllGameStats(CancellationToken ct)
    {
        return await gameService.GetAllGameStatsAsync(ct);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetGameStatsById) + "/{gameId:guid}" )]
    public async Task<ActionResult<GameStatsDto>> GetGameStatsById(Guid gameId)
    {
        return await gameService.GetGameStatsByIdAsync(gameId);
    }

    [HttpGet]
    [Route(nameof(CanAcceptNewBoards) + "/{gameId:guid}" )]
    public async Task<ActionResult<bool>> CanAcceptNewBoards(Guid gameId)
    {
        return await gameService.CanAcceptNewBoardsAsync(gameId);
    }

    /* Leave out for now, just run it once during deployment, if needed allow admin to access this eventually, within reason.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route(nameof(SeedGames))]
    public async Task<IActionResult> SeedGames()
    {
        await gameService.SeedGamesAsync();
        return NoContent();
    }
    */
    
}
using Api.DTOs;
using Api.DTOs.Requests.BoardRequests;
using Api.Security;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;


[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BoardController(IBoardService boardService) : ControllerBase
{
    
    [HttpPost]
    [Route(nameof(PurchaseBoard))]
    [Authorize(Policy = "ActivePlayer", Roles = "Player")] //ensure that admin can't play
    public async Task<ActionResult<BoardPurchaseResponseDto>> PurchaseBoard(
        [FromBody] PurchaseBoardRequest request)
    {
        var userId = User.GetUserId();
        //TODO move this to service and just return it from service method
        var (board, newBalance) = await boardService.PurchaseBoardAsync(request, userId);

        return new BoardPurchaseResponseDto
        {
            Board = board,
            NewBalance = newBalance
        };
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetBoardById) + "/{boardId:guid}")]
    public async Task<ActionResult<BoardDto>> GetBoardById(Guid boardId)
    {
        return await boardService.GetBoardByIdAsync(boardId);
    }

    [HttpGet]
    [Route(nameof(GetMyBoards))]
    public async Task<ActionResult<List<BoardDto>>> GetMyBoards([FromQuery] Guid? gameId = null)
    {
        var userId = User.GetUserId();
        return await boardService.GetPlayerBoardsAsync(userId, gameId);
    }
    
    [HttpGet]
    [Route(nameof(GetMyWinningBoards))]
    public async Task<ActionResult<List<BoardDto>>> GetMyWinningBoards()
    {
        var userId = User.GetUserId();
        return await boardService.GetPlayerWinningBoardsAsync(userId);
    }

    [HttpGet]
    [Route(nameof(CanAffordBoard))]
    public async Task<ActionResult<bool>> CanAffordBoard([FromQuery] int numberCount)
    {
        var userId = User.GetUserId();
        return await boardService.CanPlayerAffordBoardAsync(userId, numberCount);
    }

    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetBoardsForGame))]
    public async Task<ActionResult<List<BoardDto>>> GetBoardsForGame(
        Guid gameId,
        [FromQuery] bool includePlayerInfo = false)
    {
        return await boardService.GetBoardsForGameAsync(gameId, includePlayerInfo);
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetWinningBoardsForGame))]
    public async Task<ActionResult<List<BoardDto>>> GetWinningBoardsForGame(Guid gameId)
    {
        return await boardService.GetWinningBoardsForGameAsync(gameId);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    [Route(nameof(DeleteBoard) + "/{boardId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBoard(Guid boardId)
    {
        var adminId = User.GetUserId();
        await boardService.DeleteBoardAsync(boardId, adminId);
        return NoContent();
    }
}
using Api.DTOs.Requests.PlayerRequests;
using Api.DTOs.Responses.PlayerResponses;
using Api.Security;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PlayerController(IPlayerService playerService) : ControllerBase
{
    
    [HttpGet]
    [Route(nameof(GetMe))]
    public async Task<ActionResult<ApplicationUserDto>> GetMe(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var dto = await playerService.GetSelfAsync(userId, ct);
        return Ok(dto);
    }

    [HttpPut]
    [Route(nameof(UpdateMe))]
    public async Task<ActionResult<ApplicationUserDto>> UpdateMe(
        [FromBody] UpdatePlayerRequest request,
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var dto = await playerService.UpdateSelfAsync(userId, request, ct);
        return Ok(dto);
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(SearchUsers))]
    public async Task<ActionResult<IReadOnlyList<ApplicationUserListItemDto>>> SearchUsers(
        [FromQuery] PlayerSearchQuery query,
        CancellationToken ct)
    {
        var list = await playerService.SearchAsync(query, ct);
        return Ok(list);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetById) + "/{userId:guid}")]
    public async Task<ActionResult<ApplicationUserDto>> GetById(Guid userId, CancellationToken ct)
    {
        var dto = await playerService.GetByIdAsync(userId, includeDeleted: true, ct);
        return Ok(dto);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [Route(nameof(UpdatePlayer) + "/{playerId:guid}")]
    public async Task<ActionResult<ApplicationUserDto>> UpdatePlayer(
        Guid playerId,
        [FromBody] UpdatePlayerAdminRequest request,
        CancellationToken ct)
    {
        var dto = await playerService.UpdatePlayerAsync(playerId, request, ct);
        return Ok(dto);
    }

    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route(nameof(SetActivityStatus) +  "/{playerId:guid}")]
    public async Task<ActionResult<ApplicationUserDto>> SetActivityStatus(
        Guid playerId,
        [FromQuery] bool status,
        CancellationToken ct)
    {
        var dto = await playerService.SetActivityStatusAsync(playerId, status, ct);
        return Ok(dto);
    }

    [HttpDelete]
    [Authorize(Roles = "Admin")]
    [Route(nameof(SoftDelete) + "/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SoftDelete(Guid userId, CancellationToken ct)
    {
        await playerService.SoftDeleteAsync(userId, ct);
        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route(nameof(Restore) + "/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Restore(Guid userId, CancellationToken ct)
    {
        await playerService.RestoreAsync(userId, ct);
        return NoContent();
    }
    
}
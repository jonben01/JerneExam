using Api.DTOs.Requests.TransactionRequests;
using Api.DTOs.Responses.TransactionResponses;
using Api.Security;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionController(ITransactionService transactionService) : ControllerBase
{
    [HttpPost]
    [Route(nameof(CreateDepositRequest))]
    [Authorize(Policy = "ActivePlayer", Roles = "Player")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateDepositRequest([FromBody] CreateDepositRequest request, 
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        await transactionService.CreateDepositRequestAsync(userId, request, ct);
        return NoContent();
    }
    
    [HttpGet]
    [Route(nameof(GetMyTransactionHistory))]
    public async Task<ActionResult<IReadOnlyList<TransactionHistoryListItemDto>>> GetMyTransactionHistory(
        CancellationToken ct)
    {
        var userId = User.GetUserId();
        var list = await transactionService.GetPersonalTransactionHistoryAsync(userId, ct);
        return Ok(list);
    }
    
    [HttpGet]
    [Route(nameof(GetMyBalance))]
    public async Task<ActionResult<int>> GetMyBalance(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var balance = await transactionService.GetBalanceAsync(userId, ct);
        return Ok(balance);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetPendingDeposits))]
    public async Task<ActionResult<IReadOnlyList<PendingDepositsListItemDto>>> GetPendingDeposits(
        CancellationToken ct)
    {
        var list = await transactionService.GetPendingDepositsListAsync(ct);
        return Ok(list);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route(nameof(ApproveDeposit) + "/{transactionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ApproveDeposit(Guid transactionId, CancellationToken ct)
    {
        var adminUserId = User.GetUserId();
        await transactionService.ApproveDepositAsync(transactionId, adminUserId, ct);
        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Route(nameof(RejectDeposit) + "/{transactionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectDeposit(Guid transactionId, CancellationToken ct)
    {
        var adminUserId = User.GetUserId();
        await transactionService.RejectDepositAsync(transactionId, adminUserId, ct);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetTransactionHistory))]
    public async Task<ActionResult<IReadOnlyList<TransactionHistoryListItemDto>>> GetTransactionHistory(
        CancellationToken ct)
    {
        var list = await transactionService.GetTransactionHistoryAsync(ct);
        return Ok(list);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetUserTransactionHistory) + "/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<TransactionHistoryListItemDto>>> GetUserTransactionHistory(
        Guid userId,
        CancellationToken ct)
    {
        var list = await transactionService.GetUserTransactionHistoryAsync(userId, ct);
        return Ok(list);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetByMobilePayReference))]
    public async Task<ActionResult<TransactionDto>> GetByMobilePayReference(
        [FromQuery] string mobilePayRef,
        CancellationToken ct)
    {
        var dto = await transactionService.GetByMobilePayReferenceAsync(mobilePayRef, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route(nameof(GetById) + "/{transactionId:guid}")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid transactionId, CancellationToken ct)
    {
        var dto = await transactionService.GetByIdAsync(transactionId, ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
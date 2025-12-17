using Api.DTOs;
using Api.DTOs.Requests.AuthRequests;
using Api.DTOs.Responses.PlayerResponses;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ForgotPasswordRequest = Api.DTOs.Requests.AuthRequests.ForgotPasswordRequest;
using LoginRequest = Api.DTOs.Requests.AuthRequests.LoginRequest;
using ResetPasswordRequest = Api.DTOs.Requests.AuthRequests.ResetPasswordRequest;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [Route(nameof(RegisterPlayer))]
    [HttpPost]
    public async Task<ActionResult<ApplicationUserDto>> RegisterPlayer([FromBody] RegisterPlayerRequest request)
    {
        return await authService.RegisterPlayerAndSendInviteAsync(request);
    }

    [AllowAnonymous]
    [Route(nameof(Login))]
    [HttpPost]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        return await authService.LoginAsync(request);
    }
    
    //Sends a new activation email to the user
    [AllowAnonymous]
    [Route(nameof(ForgotPassword))]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await authService.GeneratePasswordResetTokenAsync(request.Email);
        return NoContent();
    }
    
    //Confirms email and sets password (first-time activation)
    [AllowAnonymous]
    [Route(nameof(ConfirmEmail))]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        await authService.ConfirmEmailAndSetPassword(request);
        return NoContent();
    }
    
    //Reset password using reset token
    [AllowAnonymous]
    [Route(nameof(ResetPassword))]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await authService.ResetPasswordAsync(request);
        return NoContent();
    }
}
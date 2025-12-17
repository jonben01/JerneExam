
using Api.DTOs;
using Api.DTOs.Requests.AuthRequests;
using Api.DTOs.Responses.PlayerResponses;

namespace Api.Services.Interfaces;

public interface IAuthService
{
    Task<ApplicationUserDto> RegisterPlayerAndSendInviteAsync(
        RegisterPlayerRequest request);
    
    Task<LoginResponse> LoginAsync(LoginRequest request);
    
    Task GeneratePasswordResetTokenAsync(string email);

    Task ConfirmEmailAndSetPassword(ConfirmEmailRequest request);
    
    Task ResetPasswordAsync(ResetPasswordRequest request);
    
    

}
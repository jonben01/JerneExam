using Api.DTOs.Responses.PlayerResponses;
using DataAccess.Entities;

namespace Api.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public ApplicationUserDto User { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime Expires { get; set; }
}
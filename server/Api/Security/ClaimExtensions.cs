using System.Security.Claims;
using Api.DTOs;
using Api.DTOs.Responses.PlayerResponses;

namespace Api.Security;

public static class ClaimExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal claims)
    {
        var id = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidOperationException("No user id claim found in the token");
        }

        return Guid.Parse(id);
    }

    public static IEnumerable<Claim> ToClaims(this ApplicationUserDto user) =>
    [
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role)
    ];

    public static ClaimsPrincipal ToPrincipal(this ApplicationUserDto user) => 
        new ClaimsPrincipal(new ClaimsIdentity(user.ToClaims(), authenticationType: "Test"));
}
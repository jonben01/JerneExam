using DataAccess.Entities;

namespace Api.Security;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Api.DTOs.Requests.AuthRequests;
using Api.Services.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Tests.DatabaseUtil;

namespace Tests;

public class LoginTest
{
    private static async Task<IServiceScope> ArrangeDbAndNewScopeAsync(bool seed = true)
    {
        var scope = TestRoot.Provider.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
        await seeder.EnsureUsersSeededOnceAsync();
        await seeder.Clear();

        if (seed)
        {
            await seeder.Seed();
        }

        return scope;
    }
    
    [Fact]
    public async Task LoginAsync_HappyPath_ReturnsTokenInfo()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);
        
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //ensure user is confirmed and not deleted
        var user = await userManager.FindByEmailAsync(SeedUsers.ActiveRichEmail);
        Assert.NotNull(user);

        user!.EmailConfirmed = true;
        user.IsDeleted = false;
        user.DeletedAt = null;
        await userManager.UpdateAsync(user);

        var req = new LoginRequest
        {
            Email = SeedUsers.ActiveRichEmail,
            Password = "123456"
        };

        var res = await authService.LoginAsync(req);

        Assert.False(string.IsNullOrWhiteSpace(res.Token));
        Assert.Equal("Player", res.Role);

        Assert.Equal(SeedUsers.ActiveRichId, res.User.Id);
        Assert.Equal(SeedUsers.ActiveRichEmail, res.User.Email);

        //check jwt values against user values
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(res.Token);
        Assert.Equal(SeedUsers.ActiveRichId.ToString(), jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(SeedUsers.ActiveRichEmail, jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Contains(jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value), r => r == "Player");
    }
    
    
    [Fact]
    public async Task LoginAsync_WrongCredentials_Throws()
    {
        using var scope = await ArrangeDbAndNewScopeAsync(seed: false);
        
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //ensure user is confirmed and not deleted
        var user = await userManager.FindByEmailAsync(SeedUsers.ActiveRichEmail);
        Assert.NotNull(user);

        user!.EmailConfirmed = true;
        user.IsDeleted = false;
        user.DeletedAt = null;
        await userManager.UpdateAsync(user);

        var req = new LoginRequest
        {
            Email = SeedUsers.ActiveRichEmail,
            Password = "WRONG_PASSWORD"
        };

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync(req));

        Assert.Equal(
            "Invalid email or password. \n (Make sure you've activated your account via email)",
            ex.Message);
    }
}
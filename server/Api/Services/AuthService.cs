using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Api.DTOs;
using Api.DTOs.Requests;
using Api.DTOs.Requests.AuthRequests;
using Api.DTOs.Responses.PlayerResponses;
using Api.DTOs.Util;
using Api.Security;
using Api.Services.Email;
using Api.Services.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Services;

//TODO figure out if i should rate limit anything that could cause user enumeration - time differences between error messages
public class AuthService : IAuthService
{
    
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _tokenService = tokenService;
        _configuration = configuration;
    }
    
    public async Task<ApplicationUserDto> RegisterPlayerAndSendInviteAsync(RegisterPlayerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validator.ValidateObject(request, new ValidationContext(request), true);

        var player = new ApplicationUser
        {
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            //TODO decide if activating here is better than when the user makes their pw
            IsActivePlayer = true,
            IsDeleted = false,
            //used for first login verification
            EmailConfirmed = false,
        };

        //create a user without password
        var createResult = await _userManager.CreateAsync(player);
        
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }
        
        var roleResult = await _userManager.AddToRoleAsync(player, "Player");
        if (!roleResult.Succeeded)
        {
            var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            
            var deleteResult = await _userManager.DeleteAsync(player);

            if (deleteResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to add role: {roleErrors}");
            }
            
            var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to add role and delete user: {errors}");
        }
        
        //generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(player);
        
        //create invite URL
        var inviteUrl = $"{UrlConstants.ActivationUrl}?userId={player.Id}&token={Uri.EscapeDataString(token)}";
        
        await _emailSender.SendEmailAsync(
            player.Email, 
            "Activate your account",
            $"Hi {player.FullName},<br/>" + 
            $"Please activate your account by clicking <a href=\"{inviteUrl}\"> this link</a>"
            );
        
        var user = player.ToDto();
        return user;
    }

    //maybe expand to including phone/username login
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validator.ValidateObject(request, new ValidationContext(request), true);
        
        //TODO refactor to helper method -- too bloated
        
        //Email already cant be white space, but let's leave some tech debt, why not.
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Email or password is empty");
        
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || user.IsDeleted)
        {
            ThrowInvalidCredentials();
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, 
            request.Password, 
            lockoutOnFailure: true
        );
        
        if (signInResult.IsLockedOut)
        {
            ThrowInvalidCredentials();
        }

        if (!signInResult.Succeeded)
        {
            ThrowInvalidCredentials();
        }
        
        if (!user.EmailConfirmed)
        {
            throw new UnauthorizedAccessException("Please activate your account through email");
        }
        
        var token = await _tokenService.GenerateTokenAsync(user);
            
        var role = (await _userManager.GetRolesAsync(user))
            .DefaultIfEmpty("Player")
            .First();
            
        var jwtSettings = _configuration.GetSection("Jwt");
        var expiryMinutes = jwtSettings.GetValue<int?>("ExpiryInMinutes") ?? 60;
            
        return new LoginResponse
        {
            Token = token,
            User = user.ToDto(),
            Role = role,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }
    
    [DoesNotReturn]
    private static void ThrowInvalidCredentials()
    {
        throw new UnauthorizedAccessException("Invalid email or password. \n (Make sure you've activated your account via email)");
    }

    public async Task GeneratePasswordResetTokenAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is empty");
        }
        
        var user =  await _userManager.FindByEmailAsync(email);

        if (user is null || user.IsDeleted)
        {
            //avoid user enumeration
            return;
        }
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetUrl = $"{UrlConstants.ResetPwdUrl}?userId={user.Id}&token={Uri.EscapeDataString(token)}";

        await _emailSender.SendEmailAsync(
            user.Email,
            "Password Reset Request",
            $"Hi {user.FullName},<br/><br/>" +
            "You requested to reset your password." + 
            $"Click <a href=\"{resetUrl}\">this link</a> to reset your password. <br/><br/>" +
            "If you didn't request this, you can safely ignore this email. <br/><br/>" +
            "This link will expire in 24 hours"
        );
    }

    public async Task ConfirmEmailAndSetPassword(ConfirmEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validator.ValidateObject(request, new ValidationContext(request), true);
        
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
        {
            throw new InvalidOperationException("Invalid confirmation link");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is empty");
        }
        
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null || user.IsDeleted)
        {
            throw new ArgumentException("User not found");
        }

        if (user.EmailConfirmed)
        {
            throw new InvalidOperationException("Email already confirmed");
        }
        var token = Uri.UnescapeDataString(request.Token);
        
        var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
        
        if (!confirmResult.Succeeded)
        {
            var errors = string.Join(", ", confirmResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Email confirmation failed: {errors}");
        }
        
        var passwordResult = await _userManager.AddPasswordAsync(user, request.Password);
        if (!passwordResult.Succeeded)
        {
            var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password set failed: {errors}");
        }
        //TODO maybe send a welcome email, might be a waste though
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validator.ValidateObject(request, new ValidationContext(request), true);
        
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
        {
            throw new InvalidOperationException("Invalid reset link");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ArgumentException("New password is empty");
        }
        
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null || user.IsDeleted)
        {
            throw new ArgumentException("User not found");
        }
        var token = Uri.UnescapeDataString(request.Token);
        var resetResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        if (!resetResult.Succeeded)
        {
            var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password reset failed: {errors}");
        }
        
        //if the user somehow lost the original activation email they should be able to activate it here
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            //TODO check result of update and throw if it fails
        }
        //TODO send a confirmation email with a link like "Click here to login"
    }
}
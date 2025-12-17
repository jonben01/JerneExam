using System.ComponentModel.DataAnnotations;
using Api.DTOs.Requests.PlayerRequests;
using Api.DTOs.Responses.PlayerResponses;
using Api.DTOs.Util;
using Api.Services.Interfaces;
using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class PlayerService : IPlayerService
{
    private readonly MyDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlayerService(MyDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    public Task<ApplicationUserDto> GetSelfAsync(Guid currentUserId, CancellationToken ct = default) => 
        GetByIdAsync(currentUserId, includeDeleted: false ,ct);
    
    public async Task<ApplicationUserDto> UpdateSelfAsync(
        Guid currentUserId, 
        UpdatePlayerRequest request, 
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

        var phone = request.PhoneNumber.Trim();
        if (string.IsNullOrEmpty(phone))
        {
            throw new ArgumentException("Phone number cannot be empty");
        }
        
        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == currentUserId, ct) 
                   ?? throw new KeyNotFoundException($"User {currentUserId} doesn't exist"); //impressive if you get this

        if (user.IsDeleted)
        {
            throw new InvalidOperationException("Cannot update a deleted player, contact administrator");
        }

        if (string.Equals(user.PhoneNumber ?? string.Empty, phone, StringComparison.Ordinal))
        {
            return user.ToDto();
        }
        
        user.PhoneNumber = phone;
        user.UpdatedAt = DateTime.UtcNow;
        
        var update = await _userManager.UpdateAsync(user);
        
        if (update.Succeeded) return user.ToDto();
        
        var errors = string.Join("; ", update.Errors.Select(e => e.Description));
        throw new InvalidOperationException(errors);
    }

    public async Task<ApplicationUserDto> UpdatePlayerAsync(
        Guid playerId, 
        UpdatePlayerAdminRequest request, 
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var fullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim();
        var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        if (fullName is null && phoneNumber is null && email is null)
        {
            throw new ArgumentException("At least one field (Name, Email, Phone) must be provided");
        }
        
        var user = await  _userManager.Users.SingleOrDefaultAsync(u => u.Id == playerId, ct) 
                   ?? throw new KeyNotFoundException($"User {playerId} doesn't exist");

        if (user.IsDeleted)
        {
            throw new InvalidOperationException("Cannot update a deleted player");
        }
        
        if (fullName is not null)
        {
            user.FullName = fullName;
        }
        
        if (phoneNumber is not null)
        {
            user.PhoneNumber = phoneNumber;
        }
        
        if (email is not null)
        {
            user.Email = email;
            user.UserName = email;
            
            //TODO retrigger confirmation email and set emailConfirmed to false.
        }
        
        user.UpdatedAt = DateTime.UtcNow;
        
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new ValidationException(string.Join("; ", updateResult.Errors.Select(e => e.Description)));
        }
        return user.ToDto();
    }

    public async Task<ApplicationUserDto> GetByIdAsync(
        Guid userId, 
        bool includeDeleted = false, 
        CancellationToken ct = default)
    {
        var q = _userManager.Users
            .AsNoTracking()
            .Where(u => u.Id == userId);

        if (includeDeleted)
        {
            q = q.IgnoreQueryFilters();
        }

        var user = await q.SingleOrDefaultAsync(ct);
        
        return user == null ? throw new KeyNotFoundException($"User {userId} doesn't exist") : user.ToDto();
    }

    public async Task<IReadOnlyList<ApplicationUserListItemDto>> SearchAsync(
        PlayerSearchQuery query, 
        CancellationToken ct = default)
    {
        var users = _context.Users
            .AsNoTracking();

        if (query.IncludeDeleted)
        {
            users = users.IgnoreQueryFilters();
        }

        if (query.IsActive is not null)
        {
            users = users.Where(u => u.IsActivePlayer == query.IsActive.Value);
        }

        var q = query.Query?.Trim();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var escaped = EscapeLikePattern(q);
            var pattern = $"%{escaped}%";
            users = users.Where(u => 
                EF.Functions.ILike(u.FullName, pattern, "\\") || 
                EF.Functions.ILike(u.PhoneNumber ?? string.Empty, pattern, "\\") || 
                EF.Functions.ILike(u.Email ?? string.Empty, pattern, "\\"));
        }
        return await users
            .OrderBy(u => u.FullName)
            .ThenBy(u => u.Email)
            .Select(u => new ApplicationUserListItemDto
        {
            Id =  u.Id,
            FullName = u.FullName,
            Email = u.Email ??  string.Empty,
            PhoneNumber = u.PhoneNumber ??  string.Empty,
            IsActivePlayer = u.IsActivePlayer,
            CreatedAt = u.CreatedAt,
            DeletedAt = u.DeletedAt,
        })
            .ToListAsync(ct);
    }

    private static string EscapeLikePattern(string input)
    {
        return input.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
    }

    public async Task<ApplicationUserDto> SetActivityStatusAsync(Guid playerId, bool status, CancellationToken ct = default)
    {
        var user = await _userManager.Users
                   .SingleOrDefaultAsync(u => u.Id == playerId, ct) 
                   ?? throw new KeyNotFoundException($"User {playerId} doesn't exist");
        
        if (user.IsDeleted)
        {
            throw new InvalidOperationException("Cannot change activity status for a deleted player");
        }
        
        user.IsActivePlayer = status;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ValidationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        return user.ToDto();
    }

    public async Task SoftDeleteAsync(Guid playerId, CancellationToken ct = default)
    {
        var user = await _userManager.Users
                   .IgnoreQueryFilters()
                   .SingleOrDefaultAsync(u => u.Id == playerId, ct) 
                   ?? throw new KeyNotFoundException($"User {playerId} doesn't exist");
        
        if (user.IsDeleted)
        {
            return;
        }
        
        var now = DateTime.UtcNow;
        user.IsDeleted = true;
        user.DeletedAt = now;
        user.UpdatedAt = now;
        user.IsActivePlayer = false;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ValidationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task RestoreAsync(Guid playerId, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(u => u.Id == playerId, ct)
            ?? throw new KeyNotFoundException($"User {playerId} doesn't exist");

        if (!user.IsDeleted)
        {
            return;
        }
        
        user.IsDeleted = false;
        user.DeletedAt = null;
        user.UpdatedAt = DateTime.UtcNow;
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ValidationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
    
    //TODO, make a helper for finding user
    //var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == playerId, ct) 
    //?? throw new KeyNotFoundException($"User {playerId} doesn't exist");
    //repeated a bit too much for my liking.
}
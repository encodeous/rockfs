using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RockFS.Data.Models;
using RockFS.Models.Auth;

namespace RockFS.Services.RockFS;

public class RoleService
{
    private readonly UserManager<RockFsUser> _userManager;

    public RoleService(UserManager<RockFsUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task InitAsync(RoleManager<IdentityRole> manager)
    {
        foreach (var role in Enum.GetValues<UserRole>())
        {
            await manager.CreateAsync(new IdentityRole(role.ToString()));
        }
    }
    
    public async Task AddUserToRoleAsync(RockFsUser user, UserRole role)
    {
        await _userManager.RemoveFromRolesAsync(user,
            new[] { nameof(UserRole.Guest), nameof(UserRole.Member), nameof(UserRole.Administrator), nameof(UserRole.SystemAdministrator)});
        await _userManager.AddToRoleAsync(user, nameof(UserRole.Guest));
        if (role == UserRole.Guest) return;
        await _userManager.AddToRoleAsync(user, nameof(UserRole.Member));
        if (role == UserRole.Member) return;
        await _userManager.AddToRoleAsync(user, nameof(UserRole.Administrator));
        if (role == UserRole.Administrator) return;
        await _userManager.AddToRoleAsync(user, nameof(UserRole.SystemAdministrator));
        if (role == UserRole.SystemAdministrator) return;
    }

    public async Task<UserRole> GetHighestRoleAsync(RockFsUser user)
    {
        if (await _userManager.IsInRoleAsync(user, nameof(UserRole.SystemAdministrator)))
        {
            return UserRole.SystemAdministrator;
        }
        if (await _userManager.IsInRoleAsync(user, nameof(UserRole.Administrator)))
        {
            return UserRole.Administrator;
        }
        if (await _userManager.IsInRoleAsync(user, nameof(UserRole.Member)))
        {
            return UserRole.Member;
        }
        return UserRole.Guest;
    }

    public async Task<bool> IsUserInRole(ClaimsPrincipal user, UserRole role)
    {
        var idUser = await _userManager.GetUserAsync(user);
        if (idUser is null) return false;
        return await _userManager.IsInRoleAsync(idUser, role.ToString());
    }
}
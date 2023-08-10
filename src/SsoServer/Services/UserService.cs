using System.Security.Claims;
using Duende.IdentityServer.Extensions;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SsoServer.Infrastructures;
using SsoServer.Models;
using SsoServer.Models.Users;

namespace SsoServer.Services;

public class UserService : IUserService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApplicationUser> AddUserAsync(ApplicationUser user, string password, params string[] roles)
    {
        await _userManager.CreateAsync(user).ManageIdentityResultAsync();
        if (!password.IsNullOrEmpty())
            await _userManager.AddPasswordAsync(user, password);
        await _userManager.AddToRolesAsync(user, roles);
        return user;
    }

    public async Task<ApplicationUser> UpdateUserAsync(ApplicationUser user, params string[] roles)
    {
        await _userManager.UpdateAsync(user).ManageIdentityResultAsync();
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles).ManageIdentityResultAsync();
        await _userManager.AddToRolesAsync(user, roles).ManageIdentityResultAsync();
        return user;
    }

    public async Task ChangePasswordAsync(string userName, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByNameAsync(userName);
        await _userManager
            .ChangePasswordAsync(user, currentPassword, newPassword)
            .ManageIdentityResultAsync();
    }

    public async Task<IList<ApplicationUser>> GetAllUserAsync()
    {
        var users =  await _userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            user.Clients = claims.Where(x => x.Type == JwtClaimTypes.ClientId)?.Select(x => x.Value).ToList() ?? new List<string>();
        }

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            user.Roles = roles?.ToList() ?? new List<string>();
        }
        
        return users;
    }

    public async Task<IList<ApplicationUser>> GetAllUserByClientIdAsync(string clientId)
    {
        var users = await _userManager.GetUsersForClaimAsync(new Claim(JwtClaimTypes.ClientId, clientId));
        
        foreach (var user in users)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            user.Clients = claims.Where(x => x.Type == JwtClaimTypes.ClientId)?.Select(x => x.Value).ToList() ?? new List<string>();
        }

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            user.Roles = roles?.ToList() ?? new List<string>();
        }

        return users;
    }

    public async Task AssociateUserToClientId(string userName, string clientId)
    {

        var user = await _userManager.FindByNameAsync(userName);
        await _userManager
            .AddClaimAsync(user, new Claim(JwtClaimTypes.ClientId, clientId))
            .ManageIdentityResultAsync();
     
    }

    public async Task RemoveUserFromClientAsync(string userName, string clientId)
    {
        var user = await _userManager.FindByNameAsync(userName);
        await _userManager.RemoveClaimAsync(user, new Claim(JwtClaimTypes.ClientId, clientId));
    }

    public async Task DeleteUserAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        await _userManager
            .DeleteAsync(user)
            .ManageIdentityResultAsync();
        
    }

}
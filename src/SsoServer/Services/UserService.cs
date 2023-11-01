using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SsoServer.Infrastructures;
using SsoServer.Models;
using SsoServer.Models.Users;

namespace SsoServer.Services;

public class UserService : IUserService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailSender _emailSender;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IHttpContextAccessor httpContextAccessor,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _httpContextAccessor = httpContextAccessor;
        _emailSender = emailSender;
        _signInManager = signInManager;
    }

    public async Task<List<string>> GetClientsByUser(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
        if (!result.Succeeded) throw new Exception();
        var claims = await _userManager.GetClaimsAsync(user);
        var clients = claims.Where(x => x.Type == JwtClaimTypes.ClientId)?.Select(x => x.Value)?.ToList() ??
                      new List<string>();
        return clients;

    }

    public async Task<ApplicationUser> AddUserAsync(ApplicationUser user, string password, params string[] roles)
    {
        var u = await _userManager.FindByNameAsync(user.UserName);
        if (u != null) return u;
        await _userManager.CreateAsync(user).ManageIdentityResultAsync();
        if (!password.IsNullOrEmpty())
            await _userManager.AddPasswordAsync(user, password);
        await _userManager.AddToRolesAsync(user, roles);
        await ResetPasswordAsync(user.UserName);
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
        await _emailSender.SendEmailAsync(
               user.Email,
               $"Associazione comune {clientId}",
               $"Il tuo account è adesso associato al comune {clientId}.");


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

    public async Task ConfirmationAccountAsync(string name, string code)
    {
        var user = await _userManager.FindByNameAsync(name);
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (!result.Succeeded)
            throw new Exception();
    }


    public async Task ResetPasswordAsync(string username)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(username);
            }
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{GetBaseUrl()}/confermaResetPassword?code={code}&email={user.Email.Trim()}";
            await _emailSender.SendEmailAsync(
                user.Email.Trim(),
                "Reset Password",
                $"Per favore resetta la tua password  <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Cliccando qui</a>.");
        }
        catch (Exception ex)
        {

        }
    }


    public async Task ConfirmationResetPasswordAsync(string email, string password, string code)
    {
        var user = await _userManager.FindByEmailAsync(email);
        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager
        .ResetPasswordAsync(user, code, password);
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            throw new Exception();
    }
    public string GetBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext.Request;

        var host = request.Headers[Constants.Constant.X_REFERER_HOST].First();
        return $"{request.Scheme}://{host}";
    }

}
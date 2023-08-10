using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using SsoServer.Models;
using SsoServer.Models.Users;

namespace SsoServer.Infrastructures;

public class IdentityClaimsProfileService : IProfileService
{
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityClaimsProfileService(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
    {
        _userManager = userManager;
        _claimsFactory = claimsFactory;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await _userManager.FindByIdAsync(sub);
        var principal = await _claimsFactory.CreateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var claims = principal.Claims.ToList();

        claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
        claims.Add(new Claim(ClaimTypes.Upn, user.Id));
        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        claims.Add(new Claim(JwtClaimTypes.GivenName, user.FirstName ?? string.Empty));
        claims.Add(new Claim(JwtClaimTypes.FamilyName, user.LastName ?? string.Empty));
        claims.Add(new Claim(IdentityServerConstants.StandardScopes.Email, user.Email));
        claims.Add(new Claim(JwtClaimTypes.ClientId, context.Client.ClientId));
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        context.IssuedClaims = claims;
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await _userManager.FindByIdAsync(sub);
        var claims = await _userManager.GetClaimsAsync(user);
        
        context.IsActive = user != null && claims.Any(x => x.Type ==  JwtClaimTypes.ClientId &&  x.Value == context.Client.ClientId);
    }
}
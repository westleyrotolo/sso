using AutoMapper;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SsoServer.Dtos.User;
using SsoServer.Features;
using SsoServer.Models;
using SsoServer.Models.Users;
using SsoServer.Services;

namespace SsoServer.Controllers;

[Authorize(Roles = $"{Constants.UserRoles.SuperAdministrator}, {Constants.UserRoles.SuperAdministrator}", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController : BaseController
{
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public UserController(
        IMapper mapper, 
        IUserService userService)
    {
        _mapper = mapper;
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserSubmitDto userSubmitDto, [FromQuery]string clientId)
    {
        var user = _mapper.Map<ApplicationUser>(userSubmitDto);
        await _userService.AddUserAsync(user, userSubmitDto.Password, userSubmitDto.Roles.ToArray());
        if (!clientId.IsNullOrEmpty())
            await _userService.AssociateUserToClientId(user.UserName, clientId);
        return Ok();
    }
    [HttpPut]
    public async Task<IActionResult> UpdateUserAsync([FromBody] UserSubmitDto userSubmitDto)
    {
        var user = _mapper.Map<ApplicationUser>(userSubmitDto);
        await _userService.UpdateUserAsync(user, userSubmitDto.Roles.ToArray());
        return Ok();
    }

    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetAllUserByClientIdAsync([FromRoute]string clientId)
    {
        var users = await _userService.GetAllUserByClientIdAsync(clientId);
        return Ok(users.Select(u => _mapper.Map<UserBaseDto>(u)));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsersAsync()
    {
        var users = await _userService.GetAllUserAsync();
        return Ok(users.Select(u => _mapper.Map<UserBaseDto>(u)));
    }

    [HttpPost("changePassword")]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDto changePasswordDto)
    {
        await _userService.ChangePasswordAsync(
            changePasswordDto.UserName, 
            changePasswordDto.CurrentPassword,
            changePasswordDto.NewPassword
            );
        return Ok();
    }

    [HttpGet("associate/{userName}/{clientId}")]
    public async Task<IActionResult> AssociateUserClientAsync([FromRoute] string userName, [FromRoute] string clientId)
    {
        await _userService.AssociateUserToClientId(userName, clientId);
        return Ok();
    }
    [HttpGet("dissociate/{userName}/{clientId}")]
    public async Task<IActionResult> DissociateUserClientAsync([FromRoute] string userName, [FromRoute] string clientId)
    {
        await _userService.RemoveUserFromClientAsync(userName, clientId);
        return Ok();
    }

    
}
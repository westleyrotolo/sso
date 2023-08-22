using AutoMapper;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SsoServer.Dtos.Client;
using SsoServer.Dtos.User;
using SsoServer.Services;

namespace SsoServer.Controllers;

//[Authorize(Roles = Constants.UserRoles.SuperAdministrator, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ClientController : BaseController
{
    private readonly IMapper _mapper;
    private readonly IClientService _clientService;
    private readonly IUserService _userService;

    public ClientController(
        IMapper mapper,
        IClientService clientService,
        IUserService userService)
    {
        _mapper = mapper;
        _clientService = clientService;
        _userService = userService;
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateClientAsync([FromBody]ClientSubmitDto clientSubmitDto)
    {
        var client = _mapper.Map<Client>(clientSubmitDto);
        client = await _clientService.AddClientAsync(client.ClientId, client.ClientName, client.Description);
        return Ok(_mapper.Map<ClientDto>(client));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateClientAsync([FromBody] ClientSubmitDto clientSubmitDto)
    {
        var client = _mapper.Map<Client>(clientSubmitDto);
        client = await _clientService.UpdateClientAsync(client.ClientId, client.ClientName, client.Description);
        return Ok(_mapper.Map<ClientDto>(client));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllClientsAsync()
    {
        var clients = await _clientService.GetAllClientAsync();
        return Ok(clients.Select(c => _mapper.Map<ClientBaseDto>(c)));
    }

    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] string clientId)
    {
        var client = await _clientService.GetByIdAsync(clientId);
        var users = await _userService.GetAllUserByClientIdAsync(clientId);
        var clientDto = _mapper.Map<ClientDto>(client);
        if (users != null && users.Count > 0)
        {
            var usersDto = users.Select(x => _mapper.Map<UserBaseDto>(x));
            clientDto.Users = usersDto.ToList();
        }
        return Ok(_mapper.Map<ClientDto>(clientDto));
    }
}
using AutoMapper;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SsoServer.Dtos.Client;
using SsoServer.Services;

namespace SsoServer.Controllers;

[Authorize(Roles = Constants.UserRoles.SuperAdministrator, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ClientController : BaseController
{
    private readonly IMapper _mapper;
    private readonly IClientService _clientService;

    public ClientController(IMapper mapper, IClientService clientService)
    {
        _mapper = mapper;
        _clientService = clientService;
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateClientAsync([FromBody]ClientSubmitDto clientSubmitDto)
    {
        var client = _mapper.Map<Client>(clientSubmitDto);
        await _clientService.AddClientAsync(client);
        return Ok(_mapper.Map<ClientDto>(client));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateClientAsync([FromBody] ClientSubmitDto clientSubmitDto)
    {
        var client = _mapper.Map<Client>(clientSubmitDto);
        await _clientService.UpdateClientAsync(client);
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
        return Ok(_mapper.Map<ClientDto>(client));
    }
}
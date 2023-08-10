using AutoMapper;
using Duende.IdentityServer.Models;
using SsoServer.Dtos.Client;

namespace SsoServer.Models.Clients;

public class ClientMapper : Profile
{
    public ClientMapper()
    {
        CreateMap<Client, ClientDto>();
        CreateMap<Client, ClientBaseDto>();
        CreateMap<ClientSubmitDto, Client>();
    }
}
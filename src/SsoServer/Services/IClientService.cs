using Duende.IdentityServer.Models;

namespace SsoServer.Services;

public interface IClientService
{
    Task<Client> AddClientAsync(Client client);
    Task<Client> UpdateClientAsync(Client client);
    Task<List<Client>> GetAllClientAsync();
    Task<Client> GetByIdAsync(string clientId);
    Task DeleteAsync(string clientId);
}
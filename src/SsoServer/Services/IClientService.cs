using Duende.IdentityServer.Models;

namespace SsoServer.Services;

public interface IClientService
{
    Task<Client> AddClientAsync(string clientId, string clientName, string clientDescritpion);
    Task<Client> UpdateClientAsync(string clientId, string clientName, string clientDescritpion);
    Task<List<Client>> GetAllClientAsync();
    Task<Client> GetByIdAsync(string clientId);
    Task DeleteAsync(string clientId);
}
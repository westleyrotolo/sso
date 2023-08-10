using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace SsoServer.Services;

public class ClientService : IClientService
{
    private readonly ConfigurationDbContext _configurationDbContext;
    public ClientService(ConfigurationDbContext configurationDbContext)
    {
        _configurationDbContext = configurationDbContext ?? throw new ArgumentNullException(nameof(configurationDbContext));
    }
    public  Task<Client> AddClientAsync(Client client)
    {
        return TryAddClient(client);
    }

    public async Task<Client> UpdateClientAsync(Client client)
    {
        _configurationDbContext.Clients.Update(client.ToEntity());
        await _configurationDbContext.SaveChangesAsync();
        return client;
    }

    public async Task<List<Client>> GetAllClientAsync()
    {
        var clients = await _configurationDbContext.Clients.ToListAsync();
        return clients.Select(x => x.ToModel()).ToList();
    }

    public async Task<Client> GetByIdAsync(string clientId)
    {
        var client = await _configurationDbContext.Clients.FindAsync(clientId);
        return client.ToModel();
    }

    public async Task DeleteAsync(string clientId)
    {
        var client = await _configurationDbContext.Clients.FindAsync(clientId);
        if (client == null) return;
        _configurationDbContext.Clients.Remove(client);
        await _configurationDbContext.SaveChangesAsync();
    }

    private async Task<Client> TryAddClient(Client client)
    {
        if (!_configurationDbContext.Clients.Any(x => x.ClientId == client.ClientId))
        {
            client = _configurationDbContext.Clients.Add(client.ToEntity()).Entity.ToModel();
        }
        await _configurationDbContext.SaveChangesAsync();
        Log.Information($"Successfully added client: {client.ClientName}");
        return client;
    }
}